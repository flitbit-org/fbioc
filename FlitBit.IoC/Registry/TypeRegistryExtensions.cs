#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using FlitBit.Emit;

namespace FlitBit.IoC.Registry
{
	internal static class TypeRegistryExtensions
	{
		internal static ITypeRegistration DynamicRegister<T>(this ITypeRegistry<T> reg)
		{
			var m = typeof(ITypeRegistry<T>)
				.GetGenericMethod("Register", 0, 1)
				.MakeGenericMethod(typeof(T));

			return (ITypeRegistration)m.Invoke(reg, null);
		}
		internal static ITypeRegistration DynamicRegister<T>(this ITypeRegistry<T> reg, Type c)
		{
			var m = typeof(ITypeRegistry<T>)
				.GetGenericMethod("Register", 0, 1)
				.MakeGenericMethod(c);

			return (ITypeRegistration)m.Invoke(reg, null);
		}
	}

}
