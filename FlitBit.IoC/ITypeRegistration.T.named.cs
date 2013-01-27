#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;

namespace FlitBit.IoC
{
	/// <summary>
	/// Named type registration for type T.
	/// </summary>
	/// <typeparam name="T">type T</typeparam>
	public interface INamedTypeRegistration<T> : ITypeRegistration<T>, INamedRegistration
	{
	}	
}
