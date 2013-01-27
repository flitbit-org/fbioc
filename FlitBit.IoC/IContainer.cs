#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using FlitBit.Core;


namespace FlitBit.IoC
{
	/// <summary>
	/// Indicates how a container should track the lifespan an 
	/// object it creates.
	/// </summary>
	public enum LifespanTracking
	{
		/// <summary>
		/// Default tracking == Automatic.
		/// </summary>
		Default = 0,
		/// <summary>
		/// Indicates the container must automatically track the lifespans
		/// and ensure IDisposable instances are disposed.
		/// </summary>
		Automatic = 0,
		/// <summary>
		/// Indicates the instances are externally tracked. Callers are
		/// responsible for cleaning up IDisposable instances.
		/// </summary>
		External = 1,
	}

	/// <summary>
	/// Container interface.
	/// </summary>
	public interface IContainer : IDisposable
	{
		/// <summary>
		/// Gets the container's unique ID.
		/// </summary>
		Guid Key { get; }

		/// <summary>
		/// Gets the container's registry.
		/// </summary>
		IContainerRegistry Registry { get; }
		
		/// <summary>
		/// Indicates whether the container is the root.
		/// </summary>
		bool IsRoot { get; }
		
		/// <summary>
		/// Indicates whether the container is within a tenant scope.
		/// </summary>
		bool IsTenant { get; }
		
		/// <summary>
		/// Gets the current tenant identifier if the container is
		/// within a tentant scope; otherwise null.
		/// </summary>
		object TenantID { get; }

		/// <summary>
		/// Creates a new instance of the target type.
		/// </summary>
		/// <param name="tracking"></param>
		/// <param name="targetType"></param>
		/// <returns></returns>
		object NewUntyped(LifespanTracking tracking, Type targetType);
				
		/// <summary>
		/// Resolves type T to an instance according to it's registration.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="tracking">Lifespan tracking used for the instance
		/// if it is newly created.</param>
		/// <returns>an instance of type T</returns>
		T New<T>(LifespanTracking tracking);

		/// <summary>
		/// Resolves type T to an instance according to it's registration, utilizing the
		/// parameters given if the instance must be newly created.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="tracking">Lifespan tracking used for the instance
		/// if it is newly created.</param>
		/// <param name="parameters">Initialization parameters whose values are used
		/// if an instance must be newly created.</param>
		/// <returns>an instance of type T</returns>
		T NewWithParams<T>(LifespanTracking tracking, params Param[] parameters);

		/// <summary>
		/// Resolves type T to an instance according to a named registration.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="tracking">Lifespan tracking used for the instance
		/// if it is newly created.</param>
		/// <param name="name">the name</param>
		/// <returns>an instance of type T</returns>
		T NewNamed<T>(LifespanTracking tracking, string name);

		/// <summary>
		/// Resolves type T to an instance according to a named registration, utilizing the
		/// parameters given if the instance must be newly created.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="tracking">Lifespan tracking used for the instance
		/// if it is newly created.</param>
		/// <param name="name">the name</param>
		/// <param name="parameters">Initialization parameters whose values are used
		/// if an instance must be newly created.</param>
		/// <returns>an instance of type T</returns>
		T NewNamedWithParams<T>(LifespanTracking tracking, string name, params Param[] parameters);

		/// <summary>
		/// Resolves a specific implementation of type T according to the implementation's registration.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="tracking">Lifespan tracking used for the instance
		/// if it is newly created.</param>
		/// <param name="subtype"></param>
		/// <returns>an instance of the implementation type</returns>
		T NewImplementationOf<T>(LifespanTracking tracking, Type subtype);

		/// <summary>
		/// Makes a child container from the current container.
		/// </summary>
		/// <param name="options">options </param>
		/// <returns></returns>
		IContainer MakeChildContainer(CreationContextOptions options);

		/// <summary>
		/// Prepares the container for being shared in multiple threads.
		/// </summary>
		/// <returns></returns>
		IContainer ShareContainer();

		/// <summary>
		/// Creates a subscription to creation events against type T.
		/// </summary>
		/// <typeparam name="T">subscription target type T</typeparam>
		/// <param name="observer">An action that will be called upon creation events against type T</param>
		void Subscribe<T>(Action<Type, T, string, CreationEventKind> observer);

		/// <summary>
		/// Notifies observers of type T that a creation event occurred.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="requestedType"></param>
		/// <param name="instance"></param>
		/// <param name="name"></param>
		/// <param name="evt"></param>
		void NotifyObserversOfCreationEvent<T>(Type requestedType, T instance, string name, CreationEventKind evt);

		/// <summary>
		/// Ensures a cache is registered with the context and returns
		/// that cache.
		/// </summary>
		/// <typeparam name="K">Registration key type K</typeparam>
		/// <typeparam name="C">Cache type C</typeparam>
		/// <param name="key">registration key</param>
		/// <param name="factory">factory method that will be used to create a new cache if one is not
		/// already present.</param>
		/// <returns>a cache</returns>
		C EnsureCache<K, C>(K key, Func<C> factory);
		/// <summary>
		/// Ensures a cache is registered with the context and returns
		/// that cache.
		/// </summary>
		/// <typeparam name="K">Registration key type K</typeparam>
		/// <typeparam name="C">Cache type C</typeparam>
		/// <param name="key">registration key</param>
		/// <returns>a cache</returns>
		C EnsureCache<K, C>(K key)
			where C : new();

		/// <summary>
		/// Tries to get a cache from the creation context.
		/// </summary>
		/// <typeparam name="K">Registration key type K</typeparam>
		/// <typeparam name="C">Cache type C</typeparam>
		/// <param name="key">registration key</param>
		/// <param name="cache">output variable where the cache will be returned upon success</param>
		/// <returns>true if the cache was returned; otherwise false.</returns>
		bool TryGetCache<K, C>(K key, out C cache)
			where C : new();

		/// <summary>
		/// Gets the cleanup scope for the context.
		/// </summary>
		ICleanupScope Scope { get; }
	}

	/// <summary>
	/// IContainer extensions.
	/// </summary>
	public static class IContainerExtensions
	{
		/// <summary>
		/// Creates an interface proxy type T over the source object. (If it looks like a duck, etc, etc.)
		/// </summary>
		/// <typeparam name="T">interface type T</typeparam>
		/// <param name="c">the container</param>
		/// <param name="source">the source</param>
		/// <returns>an interface proxy (duck type) over the source</returns>
		public static T AsIf<T>(this IContainer c, object source)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Requires<ArgumentNullException>(source != null);
			Contract.Requires<ArgumentException>(typeof(T).IsInterface);
			var duck = Duck.TypeAs<T>(source);
			c.NotifyObserversOfCreationEvent(typeof(T), duck, null, CreationEventKind.DuckType);
			return duck;
		}

		/// <summary>
		/// Gets the registry for type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="c">the container</param>
		/// <returns>the registry for type T</returns>
		public static ITypeRegistry<T> ForType<T>(this IContainer c)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Assume(c.Registry != null);

			return c.Registry.ForType<T>();
		}

		/// <summary>
		/// Gets the type registry for a generic type.
		/// </summary>
		/// <param name="c">a container</param>
		/// <param name="generic">a generic type</param>
		/// <returns>a registry for the generic type</returns>
		public static ITypeRegistry ForGenericType(this IContainer c, Type generic)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Requires<ArgumentNullException>(generic != null);
			Contract.Assume(c.Registry != null);

			return c.Registry.ForGenericType(generic);
		}

		/// <summary>
		/// Resolves an instance according to a target type's registration.
		/// </summary>
		/// <returns>an instance</returns>
		public static object NewUntyped(this IContainer c, Type targetType)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Requires<ArgumentNullException>(targetType != null);
			return c.NewUntyped(LifespanTracking.Default, targetType);
		}

		/// <summary>
		/// Resolves type T to an instance according to it's registration.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>an instance of type T</returns>
		public static T New<T>(this IContainer c)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			return c.New<T>(LifespanTracking.Default);
		}

		/// <summary>
		/// Resolves type T to an instance according to it's registration, utilizing the
		/// parameters given if the instance must be newly created.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="c">a container</param>
		/// <param name="parameters">Initialization parameters whose values are used
		/// if an instance must be newly created.</param>		
		/// <returns>an instance of type T</returns>
		public static T NewWithParams<T>(this IContainer c, params Param[] parameters)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			return c.NewWithParams<T>(LifespanTracking.Default, parameters);
		}

		/// <summary>
		/// Resolves type T to an instance according to a named registration.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="c">a container</param>
		/// <param name="name">the name</param>
		/// <returns>an instance of type T</returns>
		public static T NewNamed<T>(this IContainer c, string name)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Requires<ArgumentNullException>(name != null, "name cannot be null");
			Contract.Requires<ArgumentNullException>(name.Length > 0, "name cannot be empty");

			return c.NewNamed<T>(LifespanTracking.Default, name);
		}

		/// <summary>
		/// Resolves type T to an instance according to a named registration, utilizing the
		/// parameters given if the instance must be newly created.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="c">a container</param>
		/// <param name="name">the name</param>
		/// <param name="parameters">Initialization parameters whose values are used
		/// if an instance must be newly created.</param>
		/// <returns>an instance of type T</returns>
		public static T NewNamedWithParams<T>(this IContainer c, string name, params Param[] parameters)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Requires<ArgumentNullException>(name != null, "name cannot be null");
			Contract.Requires<ArgumentNullException>(name.Length > 0, "name cannot be empty");

			return c.NewNamedWithParams<T>(LifespanTracking.Default, name, parameters);
		}

		/// <summary>
		/// Resolves a specific implementation of type T according to the implementation's registration.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="c">a container</param>
		/// <param name="implementationType">the implementation type</param>
		/// <returns>an instance of the implementation type</returns>
		public static T NewImplementationOf<T>(this IContainer c, Type implementationType)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			return c.NewImplementationOf<T>(LifespanTracking.Default, implementationType);
		}

		/// <summary>
		/// Resolves a specific implementation of type T according to the implementation's registration.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <typeparam name="C">implementetion type C</typeparam>
		/// <param name="c">a container</param>
		/// <param name="tracking">Lifespan tracking used for the instance
		/// if it is newly created.</param>
		/// <returns>an instance of the implementation type</returns>
		public static T NewImplementationOf<T, C>(this IContainer c, LifespanTracking tracking)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			return c.NewImplementationOf<T>(tracking, typeof(C));
		}


		/// <summary>
		/// Makes a child container from the current container.
		/// </summary>
		/// <returns>a child container</returns>
		public static IContainer MakeChildContainer(this IContainer c)
		{
			return MakeChildContainer(c, CreationContextOptions.None);
		}
		/// <summary>
		/// Makes a child container from the current container.
		/// </summary>
		/// <param name="c">the container</param>
		/// <param name="options">creation context options</param>
		/// <returns>a child container</returns>
		public static IContainer MakeChildContainer(this IContainer c, CreationContextOptions options)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			return c.MakeChildContainer(options);
		}
	}
}
