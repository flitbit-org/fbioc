#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.IoC
{
	/// <summary>
	///   Root, untyped interface for type registry.
	/// </summary>
	public interface ITypeRegistry : IContainerOwned
	{
		/// <summary>
		///   Indicates whether the registration can be specialized or
		///   overridden by subsequent registrations.
		/// </summary>
		bool CanSpecializeRegistration { get; }

		/// <summary>
		///   Gets the registered type.
		/// </summary>
		Type RegisteredType { get; }

		/// <summary>
		///   Gets the type's untyped resolver.
		/// </summary>
		IResolver UntypedResolver { get; }

		/// <summary>
		///   Registers the concrete type.
		/// </summary>
		/// <param name="concreteType">
		///   A concrete type to be issued by
		///   the container when instances of the registered type are resolved.
		/// </param>
		/// <param name="parameters">pre-set parameters</param>
		/// <returns>
		///   The concrete registration (can be used to specialize the
		///   registration).
		/// </returns>
		ITypeRegistration Register(Type concreteType, params Param[] parameters);

		/// <summary>
		/// Registers a factory returning an untyped instance of the registered type.
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		ITypeRegistration RegisterUntypedFactory(Func<IContainer, Param[], object> factory);
	}
}