#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.IoC
{
	/// <summary>
	///   Type registry for type T
	/// </summary>
	/// <typeparam name="T">type T</typeparam>
	public interface ITypeRegistry<T> : ITypeRegistry
	{
		/// <summary>
		///   Gets the type resolver for type T. Intended for framework use; you should never need this.
		/// </summary>
		IResolver<T> Resolver { get; }

		/// <summary>
		///   Registers a function that will provide an implementation of type T upon demand.
		/// </summary>
		/// <param name="producer">
		///   a callback function that will produce the
		///   implementation type upon demand
		/// </param>
		/// <returns>the resulting type registration</returns>
		ITypeRegistration LazyRegister(Func<Type, Type> producer);

		/// <summary>
		///   Registers an implementation for resolving instances of type T.
		/// </summary>
		/// <typeparam name="TConcrete">implementation type TConcrete</typeparam>
		/// <param name="parameters">one or more Params to be used when resolving instances of type T</param>
		/// <returns>the registration for chaining calls</returns>
		ITypeRegistration Register<TConcrete>(params Param[] parameters) where TConcrete : T;

		/// <summary>
		///   Registers a factory for use when resolving instances of type T.
		/// </summary>
		/// <typeparam name="TConcrete">implementation type TConcrete</typeparam>
		/// <param name="factory">a factory providing instances of the implementation type</param>
		/// <returns>the registration for chaining calls</returns>
		ITypeRegistration Register<TConcrete>(Func<IContainer, Param[], TConcrete> factory) where TConcrete : T;

		/// <summary>
		///   Registers an implementation for resolving instances of type T by name.
		/// </summary>
		/// <typeparam name="TConcrete">implementation type TConcrete</typeparam>
		/// <param name="name">the name</param>
		/// <param name="parameters">pre-set parameters to use when resolving instances.</param>
		/// <returns>the registration for chaining calls</returns>
		ITypeRegistration RegisterWithName<TConcrete>(string name, params Param[] parameters) where TConcrete : T;

		/// <summary>
		///   Registers a named factory registration for resolving instances of type T by name.
		/// </summary>
		/// <typeparam name="TConcrete">implementation type TConcrete</typeparam>
		/// <param name="name">the name</param>
		/// <param name="factory">a factory providing instances of type C</param>
		/// <returns>the registration for chaining calls</returns>
		ITypeRegistration RegisterWithName<TConcrete>(string name, Func<IContainer, Param[], TConcrete> factory)
			where TConcrete : T;

		/// <summary>
		///   Gets the named type resolver for type T. Intended for framework use; you should never need this.
		/// </summary>
		/// <param name="name">the type's name</param>
		/// <param name="value">variable that will hold the resolver upon success</param>
		/// <returns>true if successful; otherwise false.</returns>
		bool TryGetNamedResolver(string name, out IResolver<T> value);
	}
}