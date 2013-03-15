using System;
using System.Diagnostics.Contracts;

namespace FlitBit.IoC
{
	/// <summary>
	///   IContainer extensions.
	/// </summary>
	public static class ContainerExtensions
	{
		/// <summary>
		///   Creates an interface proxy type T over the source object. (If it looks like a duck, etc, etc.)
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
		///   Gets the type registry for a generic type.
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
		///   Gets the registry for type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="c">the container</param>
		/// <returns>the registry for type T</returns>
		public static ITypeRegistry<T> ForType<T>(this IContainer c)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Ensures(Contract.Result<ITypeRegistry<T>>() != null);
			Contract.Assume(c.Registry != null);

			return c.Registry.ForType<T>();
		}

		/// <summary>
		///   Makes a child container from the current container.
		/// </summary>
		/// <returns>a child container</returns>
		public static IContainer MakeChildContainer(this IContainer c)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Ensures(Contract.Result<IContainer>() != null);

			return MakeChildContainer(c, CreationContextOptions.None);
		}

		/// <summary>
		///   Makes a child container from the current container.
		/// </summary>
		/// <param name="c">the container</param>
		/// <param name="options">creation context options</param>
		/// <returns>a child container</returns>
		public static IContainer MakeChildContainer(this IContainer c, CreationContextOptions options)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Ensures(Contract.Result<IContainer>() != null);

			return c.MakeChildContainer(options);
		}

		/// <summary>
		///   Creates a new instance of type T as a mutation of type S, using the provided mutator.
		/// </summary>
		/// <typeparam name="T">target type T</typeparam>
		/// <typeparam name="TSource">source type S</typeparam>
		/// <param name="c">a container</param>
		/// <param name="source">a source object</param>
		/// <param name="mutator">a mutator function</param>
		/// <returns>an instance of type T, initialized from the source object, mutated using the provided mutator.</returns>
		public static T Mutate<T, TSource>(this IContainer c, TSource source, Func<IContainer, T, T> mutator)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Requires<ArgumentNullException>(mutator != null);
			var res = NewInit<T>(c).Init(source);
			return mutator(c, res);
		}

		/// <summary>
		///   Creates a new instance of type T as a mutation of type S, using the provided mutator.
		/// </summary>
		/// <typeparam name="T">target type T</typeparam>
		/// <typeparam name="TSource">source type S</typeparam>
		/// <param name="c">a container</param>
		/// <param name="source">a source object</param>
		/// <param name="mutator">a mutator function</param>
		/// <param name="tracking">lifespan tracking for the new instance</param>
		/// <returns>an instance of type T, initialized from the source object, mutated using the provided mutator.</returns>
		public static T Mutate<T, TSource>(this IContainer c, TSource source, Func<IContainer, T, T> mutator, LifespanTracking tracking)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Requires<ArgumentNullException>(mutator != null);
			var res = NewInit<T>(c, tracking).Init(source);
			return mutator(c, res);
		}

		/// <summary>
		///   Resolves type T to an instance according to it's registration.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>an instance of type T</returns>
		public static T New<T>(this IContainer c)
		{
			Contract.Requires<ArgumentNullException>(c != null);

			return c.New<T>(LifespanTracking.Default);
		}

		/// <summary>
		///   Creates a new instance of type T, initialized from the provided source.
		/// </summary>
		/// <typeparam name="T">target type T</typeparam>
		/// <typeparam name="TSource">source type S</typeparam>
		/// <param name="c">a container</param>
		/// <param name="source">a source object</param>
		/// <returns>an instance of type T, initialized from the source object.</returns>
		public static T NewCopy<T, TSource>(this IContainer c, TSource source)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			return NewInit<T>(c, LifespanTracking.Default).Init(source);
		}

		/// <summary>
		///   Creates a new instance of type T, initialized from the provided source.
		/// </summary>
		/// <typeparam name="T">target type T</typeparam>
		/// <typeparam name="TSource">source type S</typeparam>
		/// <param name="c">a container</param>
		/// <param name="source">a source object</param>
		/// <param name="tracking">tracking</param>
		/// <returns>an instance of type T, initialized from the source object.</returns>
		public static T NewCopy<T, TSource>(this IContainer c, TSource source, LifespanTracking tracking)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			return NewInit<T>(c, tracking).Init(source);
		}

		/// <summary>
		///   Resolves a specific implementation of type T according to the implementation's registration.
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
		///   Resolves a specific implementation of type T according to the implementation's registration.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <typeparam name="TConcrete">implementetion type C</typeparam>
		/// <param name="c">a container</param>
		/// <param name="tracking">
		///   Lifespan tracking used for the instance
		///   if it is newly created.
		/// </param>
		/// <returns>an instance of the implementation type</returns>
		public static T NewImplementationOf<T, TConcrete>(this IContainer c, LifespanTracking tracking)
		{
			Contract.Requires<ArgumentNullException>(c != null);

			return c.NewImplementationOf<T>(tracking, typeof(TConcrete));
		}

		/// <summary>
		///   Creates a new instance of type T for initialization.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="c"></param>
		/// <returns></returns>
		public static Initialize<T> NewInit<T>(this IContainer c)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Ensures(Contract.Result<Initialize<T>>() != null);

			return NewInit<T>(c, LifespanTracking.Default);
		}

		/// <summary>
		///   Creates a new instance of type T for initialization.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="c"></param>
		/// <param name="tracking"></param>
		/// <returns></returns>
		public static Initialize<T> NewInit<T>(this IContainer c, LifespanTracking tracking)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Ensures(Contract.Result<Initialize<T>>() != null);

			return new Initialize<T>(c, c.New<T>(tracking));
		}

		/// <summary>
		///   Creates a new instance of type T and initializes it using the provided initializer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="c"></param>
		/// <param name="initializer"></param>
		/// <returns></returns>
		public static T NewInit<T>(this IContainer c, Func<IContainer, T, T> initializer)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Requires<ArgumentNullException>(initializer != null);

			return initializer(c, c.New<T>(LifespanTracking.Default));
		}

		/// <summary>
		///   Creates a new instance of type T and initializes it using the provided initializer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="c"></param>
		/// <param name="initializer"></param>
		/// <param name="tracking"></param>
		/// <returns></returns>
		public static T NewInit<T>(this IContainer c, Func<IContainer, T, T> initializer, LifespanTracking tracking)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Requires<ArgumentNullException>(initializer != null);

			return initializer(c, c.New<T>(tracking));
		}

		/// <summary>
		///   Resolves type T to an instance according to a named registration.
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
		///   Resolves type T to an instance according to a named registration, utilizing the
		///   parameters given if the instance must be newly created.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="c">a container</param>
		/// <param name="name">the name</param>
		/// <param name="parameters">
		///   Initialization parameters whose values are used
		///   if an instance must be newly created.
		/// </param>
		/// <returns>an instance of type T</returns>
		public static T NewNamedWithParams<T>(this IContainer c, string name, params Param[] parameters)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Requires<ArgumentNullException>(name != null, "name cannot be null");
			Contract.Requires<ArgumentNullException>(name.Length > 0, "name cannot be empty");

			return c.NewNamedWithParams<T>(LifespanTracking.Default, name, parameters);
		}

		/// <summary>
		///   Resolves an instance according to a target type's registration.
		/// </summary>
		/// <returns>an instance</returns>
		public static object NewUntyped(this IContainer c, Type targetType)
		{
			Contract.Requires<ArgumentNullException>(c != null);
			Contract.Requires<ArgumentNullException>(targetType != null);
			Contract.Ensures(Contract.Result<object>() != null);

			return c.NewUntyped(LifespanTracking.Default, targetType);
		}

		/// <summary>
		///   Resolves type T to an instance according to it's registration, utilizing the
		///   parameters given if the instance must be newly created.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="c">a container</param>
		/// <param name="parameters">
		///   Initialization parameters whose values are used
		///   if an instance must be newly created.
		/// </param>
		/// <returns>an instance of type T</returns>
		public static T NewWithParams<T>(this IContainer c, params Param[] parameters)
		{
			Contract.Requires<ArgumentNullException>(c != null);

			return c.NewWithParams<T>(LifespanTracking.Default, parameters);
		}
	}
}