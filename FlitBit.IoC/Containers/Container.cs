#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using FlitBit.Core;
using FlitBit.Core.Factory;
using FlitBit.Core.Meta;
using FlitBit.Core.Parallel;
using FlitBit.Emit;
using FlitBit.IoC.Properties;
using FlitBit.IoC.Registry;

namespace FlitBit.IoC.Containers
{
	internal partial class Container
	{
		static readonly MethodInfo CachedTryAutomaticRegisterType =
			typeof(Container).MatchGenericMethod("TryAutomaticRegisterType", BindingFlags.NonPublic | BindingFlags.Instance, 1,
																					typeof(void));

		readonly CreationContextOptions _options;
		readonly IContainer _parent;
		int _disposers = 1;

		protected Container(bool isRoot)
		{
			IsRoot = isRoot;
			Key = Guid.NewGuid();
			Scope = new CleanupScope(true);
			Registry = new ContainerRegistry(this, null);
			IsTenant = false;
			TenantID = null;
			if (!isRoot)
			{
				ContextFlow.Push<IContainer>(this);
			}
		}

		protected Container(IContainer parent, CreationContextOptions options)
		{
			_parent = parent;
			_options = options;
			Key = Guid.NewGuid();
			Scope = (options.HasFlag(CreationContextOptions.InheritScope)) ? parent.Scope : new CleanupScope(true);
			Registry = new ContainerRegistry(this, parent.Registry);
			IsTenant = parent.IsTenant;
			TenantID = parent.TenantID;
			ContextFlow.Push<IContainer>(this);
		}

		protected Container(IContainer parent, CreationContextOptions options, bool isTenant, object tenantID)
		{
			_parent = parent;
			_options = options;
			Key = Guid.NewGuid();
			Scope = (options.HasFlag(CreationContextOptions.InheritScope)) ? parent.Scope : new CleanupScope(true);
			Registry = new ContainerRegistry(this, parent.Registry);
			IsTenant = isTenant;
			TenantID = tenantID;
			ContextFlow.Push<IContainer>(this);
		}

		protected override bool PerformDispose(bool disposing)
		{
			if (disposing && Interlocked.Decrement(ref _disposers) > 0)
			{
				return false;
			}
			if (disposing)
			{
				ContextFlow.TryPop<IContainer>(this);
				var scope = this.Scope;
				Util.Dispose(ref scope);
			}
			return true;
		}

		protected IContainer MakeChildContainer(CreationContextOptions options, bool isTenant, object tenantID)
		{
			return new Container(this, options, isTenant, tenantID);
		}

		bool IsStereotype<T>()
		{
			return typeof(T).IsDefined(typeof(AutoImplementedAttribute), true);
		}

		bool LateDynamicTryAutomaticRegisterType(Type type)
		{
			var dyn = CachedTryAutomaticRegisterType.MakeGenericMethod(type);
			return (bool) dyn.Invoke(this, null);
		}

		T ResolveGeneric<T>(LifespanTracking tracking)
		{
			var r = this.Registry;
			var t = typeof(T);

			var generic = t.GetGenericTypeDefinition();
			if (r.IsTypeRegistered(generic))
			{
				return ResolveGenericAsRegistered<T>(generic, tracking);
			}

			if (t.IsInterface)
			{
				foreach (var intf in t.GetInterfaces()
															.Where(i => i.IsGenericType)
															.Reverse())
				{
					generic = intf.GetGenericTypeDefinition();
					if (r.IsTypeRegistered(generic))
					{
						return ResolveGenericAsRegistered<T>(generic, tracking);
					}
				}
			}

			while (t != null && t.IsGenericType)
			{
				generic = t.GetGenericTypeDefinition();
				if (r.IsTypeRegistered(generic))
				{
					return ResolveGenericAsRegistered<T>(generic, tracking);
				}

				foreach (var intf in t.GetInterfaces()
															.Where(i => i.IsGenericType)
															.Reverse())
				{
					generic = intf.GetGenericTypeDefinition();
					if (r.IsTypeRegistered(generic))
					{
						return ResolveGenericAsRegistered<T>(generic, tracking);
					}
				}

				t = t.BaseType;
			}

			throw new ContainerException(String.Concat("Cannot resolve generic type: ", typeof(T).GetReadableFullName()));
		}

		T ResolveGenericAsRegistered<T>(Type generic, LifespanTracking tracking)
		{
			var r = Registry.ForGenericType(generic)
											.ResolverFor<T>();
			T instance;
			if (r.TryResolve(this, tracking, null, out instance))
			{
				return instance;
			}

			throw new ContainerException(String.Concat("Cannot resolve generic type: ", typeof(T).GetReadableFullName()));
		}

		bool TryAutoRegisterFromStereotype<T>()
		{
			var self = this;
			lock (typeof(T).GetLockForType())
			{
				if (
					typeof(T).GetCustomAttributes(typeof(AutoImplementedAttribute), false)
									.Cast<AutoImplementedAttribute>()
									.Any(attr => attr.GetImplementation<T>(this, (impl, factory) =>
									{
										// use the implementation type if provided
										ITypeRegistration reg;
										if (impl != null)
										{
											reg = self.ForType<T>()
																.Register(impl);
										}
										else if (factory != null)
										{
											reg = self.ForType<T>()
																.Register((c, p) => factory());
										}
										else
										{
											throw new ContainerException(
												String.Concat(attr.GetType()
																					.GetReadableFullName(),
																			" failed provide either an instance or a functor for requested type: ",
																			typeof(T).GetReadableFullName()
													));
										}
										switch (attr.RecommemdedScope)
										{
											case InstanceScopeKind.ContainerScope:
												reg.ResolveAnInstancePerScope();
												break;
											case InstanceScopeKind.Singleton:
												reg.ResolveAsSingleton();
												break;
											default:
												reg.ResolveAnInstancePerRequest();
												break;
										}
									})))
				{
					return true;
				}
			}
			return false;
		}

		bool TryAutomaticRegisterType<T>()
		{
			if (typeof(T).IsAbstract)
			{
				return false;
			}

			Registry
				.ForType<T>()
				.DynamicRegister<T>()
				.End();
			return true;
		}

		bool TryResolveWithoutRecursion<T>(LifespanTracking tracking, out T instance)
		{
			IResolver<T> r;
			if (Registry.TryGetResolverForType(out r))
			{
				return r.TryResolve(this, tracking, null, out instance);
			}
			instance = default(T);
			return false;
		}

		#region IContainer Members

		public Guid Key { get; private set; }
		public IContainerRegistry Registry { get; private set; }
		public bool IsRoot { get; private set; }
		public bool IsTenant { get; private set; }
		public object TenantID { get; private set; }

		public object NewUntyped(LifespanTracking tracking, Type targetType)
		{
			Contract.Assert(targetType != null);

			IResolver r;
			if (Registry.TryGetResolverForType(targetType, out r))
			{
				if (r.TargetType == targetType)
				{
					object instance;
					if (r.TryUntypedResolve(this, tracking, null, out instance))
					{
						return instance;
					}
				}
			}
			else if (targetType.IsClass)
			{
				if (LateDynamicTryAutomaticRegisterType(targetType))
				{
					return NewUntyped(tracking, targetType);
				}
			}
			throw new ContainerException(String.Concat("Cannot resolve type: ", targetType.GetReadableFullName()));
		}

		public T New<T>(LifespanTracking tracking)
		{
			T instance;
			if (TryResolveWithoutRecursion(tracking, out instance))
			{
				return instance;
			}
			// Fallback #1: if it is a class, see if we can construct it...
			if (typeof(T).IsClass)
			{
				if (TryAutomaticRegisterType<T>()
					&& TryResolveWithoutRecursion(tracking, out instance))
				{
					return instance;
				}
			}
			// Fallback #2: if it has a stereotype, see if we can register from stereotypical behavior...
			if (IsStereotype<T>())
			{
				if (TryAutoRegisterFromStereotype<T>()
					&& TryResolveWithoutRecursion(tracking, out instance))
				{
					return instance;
				}
			}
			// Fallback #3: if it is a generic type, see if we can resolve the generic...			
			if (typeof(T).IsGenericType)
			{
				return ResolveGeneric<T>(tracking);
			}

			throw new ContainerException(String.Concat("Cannot resolve type: ", typeof(T).GetReadableFullName()));
		}

		public T NewWithParams<T>(LifespanTracking tracking, params Param[] parameters)
		{
			IResolver<T> registration;
			if (Registry.TryGetResolverForType(out registration))
			{
				T instance;
				if (registration.TryResolve(this, tracking, null, out instance, parameters))
				{
					return instance;
				}
			}
			else
			{
				if (TryAutomaticRegisterType<T>())
				{
					return NewWithParams<T>(tracking, parameters);
				}
			}
			throw new ContainerException(String.Concat("Cannot resolve type: ", typeof(T).GetReadableFullName()));
		}

		public T NewNamed<T>(LifespanTracking tracking, string name)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);

			IResolver<T> r;
			if (Registry.TryGetNamedResolverForType(name, out r))
			{
				T instance;
				if (r.TryResolve(this, tracking, null, out instance))
				{
					return instance;
				}
			}
			throw new ContainerException(String.Concat("Cannot resolve type ", typeof(T).GetReadableFullName(), " with name: ",
																								name));
		}

		public T NewNamedWithParams<T>(LifespanTracking tracking, string name, params Param[] parameters)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);

			IResolver<T> r;
			if (Registry.TryGetNamedResolverForType(name, out r))
			{
				T instance;
				if (r.TryResolve(this, tracking, null, out instance, parameters))
				{
					return instance;
				}
			}
			throw new ContainerException(String.Concat("Cannot resolve type ", typeof(T).GetReadableFullName(), " with name: ",
																								name));
		}

		public T NewImplementationOf<T>(LifespanTracking tracking, Type subtype)
		{
			Contract.Assert(subtype != null);
			Contract.Assert(typeof(T).IsAssignableFrom(subtype), Resources.Chk_TypeMustBeAssignableToTypeofT);

			IResolver r;
			if (Registry.TryGetResolverForType(subtype, out r))
			{
				if (r.TargetType == subtype)
				{
					object instance;
					if (r.TryUntypedResolve(this, tracking, null, out instance))
					{
						return (T) instance;
					}
				}
			}
			else if (subtype.IsClass)
			{
				if (LateDynamicTryAutomaticRegisterType(subtype))
				{
					return NewImplementationOf<T>(tracking, subtype);
				}
			}
			throw new ContainerException(String.Concat("Cannot resolve type: ", typeof(T).GetReadableFullName()));
		}

		public IContainer MakeChildContainer(CreationContextOptions options)
		{
			return new Container(this, options);
		}

		public ICleanupScope Scope { get; private set; }

		public IContainer ShareContainer()
		{
			Interlocked.Increment(ref _disposers);
			return this;
		}

		T IFactory.CreateInstance<T>()
		{
			return this.New<T>();
		}

		public bool CanConstruct<T>()
		{
			IResolver<T> r;
			return Registry.TryGetResolverForType(out r);
		}

		public Type GetImplementationType<T>()
		{
			IResolver<T> r;
			if (Registry.TryGetResolverForType(out r))
			{
				return r.TargetType;
			}
			return null;
		}

		public IFactory Next { get; set; }

		public object ParallelShare()
		{
			return ShareContainer();
		}

		public void RegisterImplementationType<T, TImpl>() where TImpl : T
		{
			this.ForType<T>()
					.Register<TImpl>()
					.End();
		}

		#endregion
	}
}