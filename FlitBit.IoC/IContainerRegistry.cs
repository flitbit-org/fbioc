#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.IoC
{
	/// <summary>
	///   A container registry is used to register types and resolve those registrations.
	/// </summary>
	public interface IContainerRegistry : IContainerOwned
	{
		/// <summary>
		///   Gets the registry for a generic type.
		/// </summary>
		/// <param name="generic">the generic type</param>
		/// <returns>the registry for the generic type</returns>
		IGenericTypeRegistry ForGenericType(Type generic);

		/// <summary>
		///   Gets the registry specific to type T
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>the type registry</returns>
		ITypeRegistry<T> ForType<T>();

		/// <summary>
		/// Gets the registry specific to the <paramref name="type"/> provided.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		ITypeRegistry UntypedRegistryFor(Type type);

		/// <summary>
		///   Determines if type T is registered.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>true if type T is registered; otherwise false</returns>
		bool IsTypeRegistered<T>();

		/// <summary>
		///   Determins if a type is registered.
		/// </summary>
		/// <param name="type">the type</param>
		/// <returns>true if the type is registered; otherwise false</returns>
		bool IsTypeRegistered(Type type);

		/// <summary>
		///   Tries to get a named resolver for type T
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="name">the name</param>
		/// <param name="value">variable to hold the resolver upon success</param>
		/// <returns>true if successful; otherwise false</returns>
		bool TryGetNamedResolverForType<T>(string name, out IResolver<T> value);

		/// <summary>
		///   Tries to get the resolver for a type.
		/// </summary>
		/// <param name="type">the type</param>
		/// <param name="value">variable to hold the resolver upon success</param>
		/// <returns>true if successful; otherwise false</returns>
		bool TryGetResolverForType(Type type, out IResolver value);

		/// <summary>
		///   Tries to get the resolver for type T
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="value">variable to hold the resolver upon success</param>
		/// <returns>true if successful; otherwise false</returns>
		bool TryGetResolverForType<T>(out IResolver<T> value);

		/// <summary>
		///   Tries to get the type registry management object for a type.
		/// </summary>
		/// <param name="type">the type</param>
		/// <param name="value">variable to hold the result upon success</param>
		/// <returns>true if successful; otherwise false</returns>
		bool TryGetTypeRegistryManagement(Type type, out ITypeRegistryManagement value);
	}
}