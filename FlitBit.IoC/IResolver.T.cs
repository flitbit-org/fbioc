#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.IoC
{
	/// <summary>
	///   Interface for resolvers of target type T
	/// </summary>
	/// <typeparam name="T">target type T</typeparam>
	public interface IResolver<T> : IResolver
	{	
		/// <summary>
		///   Tries to resolve an instance of the type according to its registration.
		/// </summary>
		/// <param name="container">the container</param>
		/// <param name="tracking">lifespan tracking</param>
		/// <param name="name">registered name or null</param>
		/// <param name="instance">variable where the instance will be returned upon success</param>
		/// <param name="parameters">one or more params to be used in resolving the instance</param>
		/// <returns>true if an instance is resolved; otherwise false</returns>
		bool TryResolve(IContainer container, LifespanTracking tracking, string name, out T instance,
			params Param[] parameters);
	}
}