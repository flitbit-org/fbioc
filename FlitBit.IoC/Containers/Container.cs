#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using FlitBit.Core;
using FlitBit.Core.Factory;
using FlitBit.Core.Meta;
using FlitBit.Core.Parallel;
using FlitBit.IoC.Properties;
using FlitBit.IoC.Registry;

namespace FlitBit.IoC.Containers
{
	internal partial class Container
	{
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
			if (!disposing) return true;

			ContextFlow.TryPop<IContainer>(this);
			var scope = Scope;
			Util.Dispose(ref scope);
			return true;
		}

		protected IContainer MakeChildContainer(CreationContextOptions options, bool isTenant, object tenantID)
		{
			return new Container(this, options, isTenant, tenantID);
		}

		T ResolveGeneric<T>(LifespanTracking tracking)
		{
			var r = Registry;
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

		bool TryAutoRegisterFromStereotype(Type type)
		{
			lock (type.GetLockForType())
			{
				if (
					type.GetCustomAttributes(typeof(AutoImplementedAttribute), false)
									.Cast<AutoImplementedAttribute>()
									.Any(attr => attr.GetImplementation(this, type, (impl, factory) =>
									{
										// Use the implementation type if provided;
										// register auto-types with the root.
										ITypeRegistration reg;
										if (impl != null)
										{
											reg = IoC.Container.Root
												.Registry.UntypedRegistryFor(type)
												.Register(impl);
										}
										else if (factory != null)
										{
											reg = IoC.Container.Root
												.Registry.UntypedRegistryFor(type)
												.RegisterUntypedFactory((c, p) => factory());
										}
										else
										{
											throw new ContainerException(
												String.Concat(attr.GetType()
																					.GetReadableFullName(),
																			" failed provide either an instance or a functor for requested type: ",
																			type.GetReadableFullName()
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

		bool TryAutomaticRegisterType(Type type)
		{
			// 1. if it has a stereotype, see if we can register from stereotypical behavior...
			if (type.IsDefined(typeof(AutoImplementedAttribute), true)
				&& TryAutoRegisterFromStereotype(type))
			{
				return true;
			}

			// 2. It is concrete with public constructor...
			if (!type.IsAbstract)
			{
				Registry
					.UntypedRegistryFor(type)
					.Register(type)
					.End();
				return true;
			}

			return false;
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
				if (targetType.IsAssignableFrom(r.TargetType))
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
				if (TryAutomaticRegisterType(targetType))
				{
					return NewUntyped(tracking, targetType);
				}
			}
			else if (Next != null && Next.CanConstruct(targetType))
			{
				return Next.CreateInstance(targetType);
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
			if (TryAutomaticRegisterType(typeof(T))
				&& TryResolveWithoutRecursion(tracking, out instance))
			{
				return instance;
			}

			// Last chance; if it is generic, see if we can resolve the generic...			
			if (typeof(T).IsGenericType)
			{
				return ResolveGeneric<T>(tracking);
			}

			if (Next != null && Next.CanConstruct<T>())
			{
				return Next.CreateInstance<T>();
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
			if (TryAutomaticRegisterType(typeof(T)) && Registry.TryGetResolverForType(out registration))
			{
				T instance;
				if (registration.TryResolve(this, tracking, null, out instance, parameters))
				{
					return instance;
				}
			}
			if (Next != null && Next.CanConstruct<T>())
			{
				return Next.CreateInstance<T>();
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
						return (T)instance;
					}
				}
			}
			else if (subtype.IsClass && Registry.TryGetResolverForType(subtype, out r))
			{
				if (r.TargetType == subtype)
				{
					object instance;
					if (r.TryUntypedResolve(this, tracking, null, out instance))
					{
						return (T)instance;
					}
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

		public object CreateInstance(Type type)
		{
			return NewUntyped(LifespanTracking.Default, type);
		}

		public bool CanConstruct<T>()
		{
			IResolver<T> r;
			Type type = typeof(T);
			return IsValidType(type) && (Registry.TryGetResolverForType(out r) || TryAutomaticRegisterType(type));
		}

		public bool CanConstruct(Type type)
		{
			IResolver r;
			return IsValidType(type) && (Registry.TryGetResolverForType(type, out r) || TryAutomaticRegisterType(type));
		}

		private bool IsValidType(Type type)
		{
			return !type.IsValueType;
		}

		public Type GetImplementationType(Type type)
		{
			if (!Registry.IsTypeRegistered(type))
			{
				TryAutomaticRegisterType(type);
			}
			var reg = Registry.UntypedRegistryFor(type);
			return reg != null ? reg.UntypedResolver.TargetType : null;
		}

		public Type GetImplementationType<T>()
		{
			IResolver<T> r;
			if (Registry.TryGetResolverForType(out r))
			{
				return r.TargetType;
			}
			// register on demand...
			if (TryAutomaticRegisterType(typeof(T)) && Registry.TryGetResolverForType(out r))
			{
				return r.TargetType;
			}
			return null;
		}

		public virtual IFactory Next { get { return IoC.Container.Root.Next; } set { IoC.Container.Root.Next = value; } }

		public object ParallelShare()
		{
			return ShareContainer();
		}

		public void RegisterImplementationType<T, TImpl>() where TImpl : T
		{
			// All global registrations go to the Root container;
			// the factory mechanism doesn't support nested scopes, etc.
			IoC.Container.Root.ForType<T>()
					.Register<TImpl>()
					.End();
		}

		#endregion
	}
}