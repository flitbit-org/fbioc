#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;

namespace FlitBit.IoC
{
	/// <summary>
	/// Type registration for type T
	/// </summary>
	/// <typeparam name="T">type T</typeparam>
	public interface ITypeRegistration<T> : ITypeRegistration
	{
		/// <summary>
		/// Gets the resolver for type T.
		/// </summary>
		IResolver<T> Resolver { get; }
	}
}
