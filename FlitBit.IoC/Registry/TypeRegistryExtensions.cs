#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

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
				.MatchGenericMethod("Register", 1, typeof(ITypeRegistration<T>), typeof(Param[]))
				.MakeGenericMethod(typeof(T));

			return (ITypeRegistration) m.Invoke(reg, new object[] {null});
		}

		internal static ITypeRegistration DynamicRegister<T>(this ITypeRegistry<T> reg, Type c)
		{
			var m = typeof(ITypeRegistry<T>)
				.MatchGenericMethod("Register", 1, typeof(ITypeRegistration<T>), typeof(Param[]))
				.MakeGenericMethod(c);

			return (ITypeRegistration) m.Invoke(reg, new object[] {null});
		}
	}
}