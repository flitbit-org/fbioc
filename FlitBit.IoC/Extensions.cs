#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;

namespace FlitBit.IoC
{
	/// <summary>
	/// Various Type extensions.
	/// </summary>
	public static class TypeExtensions
	{
		/// <summary>
		/// Gets the fully qualified, human readable name for a delegate.
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static string GetFullName(this Delegate d)
		{
			Contract.Requires<ArgumentNullException>(d != null);
			Contract.Assume(d.Target != null);
			Contract.Assume(d.Method != null);
			return String.Concat(d.Target.GetType().FullName, ".", d.Method.Name, "()");
		}
	}
}
