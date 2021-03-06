﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Threading;
using FlitBit.Emit;

namespace FlitBit.IoC.Constructors
{
	internal static class ConstructorAdapter
	{
		static readonly Lazy<EmittedModule> LazyModule =
			new Lazy<EmittedModule>(() => RuntimeAssemblies.DynamicAssembly.DefineModule("Constructors", null),
															LazyThreadSafetyMode.ExecutionAndPublication
				);

		internal static EmittedModule Module
		{
			get { return LazyModule.Value; }
		}
	}
}