#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;
using FlitBit.Core;
using FlitBit.Emit;

namespace FlitBit.IoC.Registry
{
	internal sealed class TypeRegistry<T> : TypeRegistry, ITypeRegistry<T>, ITypeRegistryManagement
	{
		readonly Lazy<MethodInfo> _genericMake =
			new Lazy<MethodInfo>(
				() =>
					typeof(TypeRegistry<T>).MatchGenericMethod("MakeConcreteRegistrationFor",
																										BindingFlags.NonPublic | BindingFlags.Instance, 1, typeof(ITypeRegistration<T>),
																										typeof(Param[])), LazyThreadSafetyMode.PublicationOnly);

		readonly Lazy<ConcurrentDictionary<string, INamedTypeRegistration<T>>> _named =
			new Lazy<ConcurrentDictionary<string, INamedTypeRegistration<T>>>(LazyThreadSafetyMode.ExecutionAndPublication);

		public TypeRegistry(IContainer container)
			: this(container, null, null)
		{}

		public TypeRegistry(IContainer container, ITypeRegistration<T> current, IEnumerable<INamedTypeRegistration<T>> named)
			: base(container, typeof(T), current)
		{
			if (named != null)
			{
				foreach (var n in named)
				{
					_named.Value.TryAdd(n.Name, n);
				}
			}
		}

		ITypeRegistration<T> Current { get { return (ITypeRegistration<T>) this.UntypedRegistration; } }

		public bool TryResolve(IContainer container, LifespanTracking tracking, out T instance, params Param[] parameters)
		{
			var resolver = (IResolver<T>) UntypedResolver;
			if (resolver != null)
			{
				return resolver.TryResolve(container, tracking, null, out instance, parameters);
			}
			instance = default(T);
			return false;
		}

		public bool TryResolveNamed(IContainer container, string name, LifespanTracking tracking, out T instance)
		{
			INamedTypeRegistration<T> current;
			if (_named.IsValueCreated
				&& _named.Value.TryGetValue(name, out current))
			{
				return current.Resolver.TryResolve(container, tracking, name, out instance);
			}
			instance = default(T);
			return false;
		}

		public bool TryResolveNamed(IContainer container, string name, LifespanTracking tracking, out T instance,
			Param[] parameters)
		{
			INamedTypeRegistration<T> current;
			if (_named.IsValueCreated
				&& _named.Value.TryGetValue(name, out current))
			{
				return current.Resolver.TryResolve(container, tracking, name, out instance);
			}
			instance = default(T);
			return false;
		}

		ITypeRegistration<T> MakeConcreteRegistrationFor<TConcrete>(Param[] parameters) where TConcrete : T
		{
			ITypeRegistration<T> result;
			if (typeof(TConcrete).IsAbstract)
			{
				// Abstract types resolved via the current container...
				result = new FactoryTypeRegistration<T, TConcrete>(Container, (c, p) => c.New<TConcrete>());
			}
			else if (typeof(TConcrete).IsClass)
			{
				result =
					(ITypeRegistration<T>)
						Activator.CreateInstance(typeof(TypeRegistration<,>).MakeGenericType(typeof(T), typeof(TConcrete)), Container,
																		parameters);
			}
			else
			{
				throw new ArgumentException(
					String.Concat("Generic argument type TConcrete should be an interface or a class: typeof(",
												typeof(TConcrete).GetReadableFullName(), ") not supported."));
			}
			return result;
		}

		INamedTypeRegistration<T> MakeNamedConcreteRegistrationFor<TConcrete>(string name, Param[] parameters)
			where TConcrete : T
		{
			INamedTypeRegistration<T> result;
			if (typeof(TConcrete).IsInterface || typeof(TConcrete).IsAbstract)
			{
				// Abstract types resolved via the current container...
				result = new NamedFactoryTypeRegistration<T, TConcrete>(Container, name, (c, p) => c.New<TConcrete>());
			}
			else if (typeof(TConcrete).IsClass)
			{
				result =
					(INamedTypeRegistration<T>)
						Activator.CreateInstance(typeof(NamedTypeRegistration<,>).MakeGenericType(typeof(T), typeof(TConcrete)), Container,
																		name,
																		parameters);
			}
			else
			{
				throw new ArgumentException(String.Concat("Generic argument type C should be an interface or a class: typeof(",
																									typeof(TConcrete).GetReadableFullName(), ") not supported."));
			}
			return result;
		}

		#region ITypeRegistry<T> Members

		public override ITypeRegistration Register(Type type, params Param[] parameters)
		{
			var current = Current;
			CheckCanSpecializeRegistration(current);
			var res =
				(ITypeRegistration<T>) this._genericMake.Value.MakeGenericMethod(type)
																	.Invoke(this, new object[] {parameters});
			CheckedSetRegistration(res, current);
			return res;
		}

		public ITypeRegistration Register<TConcrete>(params Param[] parameters) where TConcrete : T
		{
			var current = Current;
			CheckCanSpecializeRegistration(current);
			ITypeRegistration<T> res;
			if (parameters != null && parameters.Length > 0)
			{
				res = MakeConcreteRegistrationFor<TConcrete>(parameters);
			}
			else
			{
				res = MakeConcreteRegistrationFor<TConcrete>(null);
			}
			CheckedSetRegistration(res, current);
			return res;
		}

		public ITypeRegistration Register<TConcrete>(Func<IContainer, Param[], TConcrete> factory) where TConcrete : T
		{
			var current = Current;
			CheckCanSpecializeRegistration(current);

			var reg = new FactoryTypeRegistration<T, TConcrete>(Container, factory);
			CheckedSetRegistration(reg, current);
			return reg;
		}

		public ITypeRegistration RegisterWithName<TConcrete>(string name, params Param[] parameters) where TConcrete : T
		{
			var res = MakeNamedConcreteRegistrationFor<TConcrete>(name, parameters);

			var registry = _named.Value;
			registry.AddOrUpdate(name, res, (s, registration) => res);
			return res;
		}

		public ITypeRegistration RegisterWithName<TConcrete>(string name, Func<IContainer, Param[], TConcrete> factory)
			where TConcrete : T
		{
			var res = new NamedFactoryTypeRegistration<T, TConcrete>(Container, name, factory);

			var registry = _named.Value;
			registry.AddOrUpdate(name, res, (s, registration) => res);
			return res;
		}

		public ITypeRegistration LazyRegister(Func<Type, Type> factory)
		{
			Contract.Assert(factory != null);

			var current = Current;
			CheckCanSpecializeRegistration(current);

			var reg = new LazyTypeRegistration<T>(Container, factory, null);
			CheckedSetRegistration(reg, current);
			return reg;
		}

		public IResolver<T> Resolver
		{
			get
			{
				var reg = Current;
				return (reg != null) ? reg.Resolver : null;
			}
		}

		public bool TryGetNamedResolver(string name, out IResolver<T> value)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);

			INamedTypeRegistration<T> current;
			if (_named.IsValueCreated
				&& _named.Value.TryGetValue(name, out current))
			{
				value = current.Resolver;
				return true;
			}
			value = default(IResolver<T>);
			return false;
		}

		public override IResolver UntypedResolver { get { return Resolver; } }

		public override ITypeRegistration RegisterUntypedFactory(Func<IContainer, Param[], object> factory)
		{
			var current = Current;
			CheckCanSpecializeRegistration(current);

			var reg = new UntypedFactoryTypeRegistration<T>(Container, factory);
			CheckedSetRegistration(reg, current);
			return reg;
		}

		#endregion

		#region ITypeRegistryManagement Members

		public ITypeRegistry MakeCopyForContainer(IContainer container)
		{
			Contract.Assert(container != null);

			if (_named.IsValueCreated)
			{
				return new TypeRegistry<T>(container, Current, _named.Value.Values);
			}
			return new TypeRegistry<T>(container, this.Current, null);
		}

		#endregion
	}
}