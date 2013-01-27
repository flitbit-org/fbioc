#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Threading;
using FlitBit.Emit;

namespace FlitBit.IoC.Constructors
{
	internal static class ConstructorAdapter
	{
		static readonly Lazy<EmittedModule> __module = new Lazy<EmittedModule>(() =>
		{ return RuntimeAssemblies.DynamicAssembly.DefineModule("Constructors", null); },
			LazyThreadSafetyMode.ExecutionAndPublication
			);
		internal static EmittedModule Module { get { return __module.Value; } }
	}
}
