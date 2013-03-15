#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;
using FlitBit.Core;
using FlitBit.IoC.Constructors;

namespace FlitBit.IoC.Registry
{
	internal sealed class LazyTypeRegistration<T> : TypeRegistration<T>
	{
		readonly Func<Type, Type> _factory;
		readonly Object _lock = new Object();
		readonly Param[] _parameters;
		readonly Lazy<IResolver<T>> _resolver;
		Type _concreteType;

		internal LazyTypeRegistration(IContainer container, Func<Type, Type> factory, Param[] parameters)
			: base(container)
		{
			Contract.Requires<ArgumentNullException>(factory != null);

			_factory = factory;
			_resolver = new Lazy<IResolver<T>>(ConfigureResolver, LazyThreadSafetyMode.ExecutionAndPublication);
			_parameters = parameters;
		}

		public override IResolver<T> Resolver
		{
			get { return _resolver.Value; }
		}

		public override Type TargetType
		{
			get { return Util.LazyInitializeWithLock(ref _concreteType, _lock, ResolveType); }
		}

		public override IResolver UntypedResolver
		{
			get { return _resolver.Value; }
		}

		protected override IResolver<T> ConfigureResolver()
		{
			var m = typeof(LazyTypeRegistration<T>)
				.GetMethod("StrongConfigureResolver", BindingFlags.Instance | BindingFlags.NonPublic)
				.MakeGenericMethod(TargetType);
			return (IResolver<T>) m.Invoke(this, null);
		}

		protected override IResolver<T> ConstructPerRequestResolver() { throw new NotImplementedException(); }

		protected override IResolver<T> ConstructPerScopeResolver() { throw new NotImplementedException(); }

		protected override IResolver<T> ConstructSingletonResolver() { throw new NotImplementedException(); }
		Type ResolveType() { return _factory(typeof(T)); }

		internal IResolver<T> StrongConfigureResolver<TConcrete>()
			where TConcrete : class, T
		{
			var behavior = this.ScopeBehavior;
			var ctors = new ConstructorSet<T, TConcrete>(_parameters);

			if (behavior.HasFlag(ScopeBehavior.Singleton))
			{
				return new SingletonResolver<T, TConcrete>(Container, ctors);
			}
			if (behavior.HasFlag(ScopeBehavior.InstancePerScope))
			{
				return new InstancePerScopeResolver<T, TConcrete>(ctors);
			}
			return new Resolver<T, TConcrete>(ctors);
		}
	}
}