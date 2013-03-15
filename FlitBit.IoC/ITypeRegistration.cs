#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.IoC
{
	/// <summary>
	///   Registration for a type.
	/// </summary>
	public interface ITypeRegistration : IContainerRegistrationParticipant
	{
		/// <summary>
		///   Indicates whether the registration is named.
		/// </summary>
		bool IsNamed { get; }

		/// <summary>
		///   The registered type.
		/// </summary>
		Type RegisteredType { get; }

		/// <summary>
		///   Gets the registration's scope behavior.
		/// </summary>
		ScopeBehavior ScopeBehavior { get; }

		/// <summary>
		///   The target type.
		/// </summary>
		Type TargetType { get; }

		/// <summary>
		///   Gets the registered type's untyped resolver.
		/// </summary>
		IResolver UntypedResolver { get; }

		/// <summary>
		///   Indicates that a type hsould be resolved per request.
		/// </summary>
		/// <returns>the registration (for chaining)</returns>
		ITypeRegistration DisallowSpecialization();

		/// <summary>
		///   Indicates that a type hsould be resolved per request.
		/// </summary>
		/// <returns>the registration (for chaining)</returns>
		ITypeRegistration ResolveAnInstancePerRequest();

		/// <summary>
		///   Indicates that a type hsould be resolved per request.
		/// </summary>
		/// <returns>the registration (for chaining)</returns>
		ITypeRegistration ResolveAnInstancePerScope();

		/// <summary>
		///   Indicates that a type hsould be resolved per request.
		/// </summary>
		/// <returns>the registration (for chaining)</returns>
		ITypeRegistration ResolveAsSingleton();
	}
}