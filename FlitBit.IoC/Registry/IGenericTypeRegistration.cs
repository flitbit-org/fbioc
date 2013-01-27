#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

namespace FlitBit.IoC.Registry
{
	/// <summary>
	/// Registration for a generic type.
	/// </summary>
	public interface IGenericTypeRegistration : ITypeRegistration
	{
		/// <summary>
		/// Gets a resolver for type T
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>the resolver for type T</returns>
		IResolver<T> ResolverFor<T>();
	}
}
