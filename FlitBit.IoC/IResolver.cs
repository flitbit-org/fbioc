using System;

namespace FlitBit.IoC
{
	/// <summary>
	///   Interface for resolvers.
	/// </summary>
	public interface IResolver
	{
		/// <summary>
		///   Gets the type used by the resolver to fullfil resolve requests.
		/// </summary>
		Type TargetType { get; }
		
		/// <summary>
		///   Tries to resolve an instance of the type according to its registration.
		/// </summary>
		/// <param name="container">the container</param>
		/// <param name="tracking">lifespan tracking</param>
		/// <param name="name">registered name or null</param>
		/// <param name="instance">variable where the instance will be returned upon success</param>
		/// <param name="parameters">one or more params to be used in resolving the instance</param>
		/// <returns>true if an instance is resolved; otherwise false</returns>
		bool TryUntypedResolve(IContainer container, LifespanTracking tracking, string name, out object instance,
			params Param[] parameters);
	}
}