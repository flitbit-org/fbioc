#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using FlitBit.IoC.Properties;

namespace FlitBit.IoC
{
	/// <summary>
	/// Utility class for working with containers.
	/// </summary>
	public static class Create
	{
		/// <summary>
		/// Creates a new container scoped by the current container.
		/// </summary>
		/// <returns></returns>
		public static IContainer NewContainer()
		{
			return NewContainer(CreationContextOptions.None);
		}
		/// <summary>
		/// Creates a new container scoped by the current container.
		/// </summary>
		/// <param name="options">creation context options</param>
		/// <returns></returns>		
		/// <see cref="CreationContextOptions"/>
		public static IContainer NewContainer(CreationContextOptions options)
		{
			return Container.Current.MakeChildContainer(options);
		}
		
		/// <summary>
		/// Creates a tenant container.
		/// </summary>
		/// <returns></returns>
		public static IContainer TenantContainer()
		{
			return Container.Root.ResolveCurrentTenant();
		}

		/// <summary>
		/// Creates an interface proxy type T over the source object. (If it looks like a duck, etc, etc.)
		/// </summary>
		/// <typeparam name="T">interface type T</typeparam>
		/// <param name="source">the source</param>
		/// <returns>an interface proxy (duck type) over the source</returns>
		public static T AsIf<T>(object source)
		{
			Contract.Requires<ArgumentNullException>(source != null);
			Contract.Requires<ArgumentException>(typeof(T).IsInterface, Resources.Chk_TypeofTIsInterface);
			return Container.Current.AsIf<T>(source);
		}

		/// <summary>
		/// Resolves an instance of type T from the container.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>a resolved instance of type T</returns>
		/// <remarks>If type T implements IDisposable it is the caller's 
		/// responsibility to ensure that the Dispose method is called
		/// at the appropriate time. To change this behavior call the
		/// overloaded New method and supply an alternate LifespanTracking value.</remarks>
		public static T New<T>()
		{
			return Container.Current
				.New<T>(LifespanTracking.External);
		}
		/// <summary>
		/// Resolves an instance of type T from the container.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="tracking">lifespan tracking</param>
		/// <returns>a resolved instance of type T</returns>
		public static T New<T>(LifespanTracking tracking)
		{
			return Container.Current
				.New<T>(tracking);
		}
		/// <summary>
		/// Resolves a specific implementation of type T from the container.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="impl">implementation type</param>
		/// <returns>a resolved instance of type T</returns>
		public static T New<T>(Type impl)
		{
			return Container.Current
				.NewImplementationOf<T>(LifespanTracking.External, impl);
		}
		/// <summary>
		/// Resolves an instance of type T from the container.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="tracking">lifespan tracking</param>
		/// <param name="parameters">the parameters</param>
		/// <returns>a resolved instance of type T</returns>
		public static T NewWithParams<T>(LifespanTracking tracking, params Param[] parameters)
		{
			return Container.Current
				.NewWithParams<T>(tracking, parameters);
		}
		/// <summary>
		/// Resolves a specific implementation of type T from the container.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="tracking">lifespan tracking</param>
		/// <param name="impl">implementation type</param>
		/// <returns>a resolved instance of type T</returns>
		public static T New<T>(LifespanTracking tracking, Type impl)
		{
			return Container.Current
				.NewImplementationOf<T>(tracking, impl);
		}

        /// <summary>
        /// Creates a new instance of type T for initialization.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Initialize<T> NewInit<T>()
        {            
            return Container.Current
                .NewInit<T>(LifespanTracking.External);
        }

        /// <summary>
        /// Creates a new instance of type T for initialization.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tracking"></param>
        /// <returns></returns>
        public static Initialize<T> NewInit<T>(LifespanTracking tracking)
        {
            return Container.Current
                .NewInit<T>(tracking);
        }
		/// <summary>
		/// Resolves a named instance of type T from the container.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="name">the instance's name</param>
		/// <returns>a resolved instance of type T</returns>
		public static T NewNamed<T>(string name)
		{
			return Container.Current
				.NewNamed<T>(LifespanTracking.External, name);
		}
		/// <summary>
		/// Resolves a named instance of type T from the container.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="name">the instance's name</param>
		/// <param name="tracking">lifespan tracking</param>
		/// <returns>a resolved instance of type T</returns>
		public static T NewNamed<T>(string name, LifespanTracking tracking)
		{
			return Container.Current
				.NewNamed<T>(tracking, name);
		}

		/// <summary>
		/// Gets a container with its own scope; either an existing
		/// container that is not root or a new child container.
		/// </summary>
		/// <returns>a container</returns>
		public static IContainer SharedOrNewContainer()
		{
			if (Container.Current.IsRoot)
			{
				return Container.Current.MakeChildContainer();
			}
			else
			{
				return Container.Current.ShareContainer();
			}
		}				
	}
}
