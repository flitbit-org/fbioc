#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using FlitBit.Core;

namespace FlitBit.IoC
{
	/// <summary>
	/// Options for creation contexts.
	/// </summary>
	[Flags]
	public enum CreationContextOptions
	{
		/// <summary>
		/// No options.
		/// </summary>
		None = 0,
		/// <summary>
		/// Indicates the creation context should track instances.
		/// </summary>
		InstanceTracking = 1,
		/// <summary>
		/// Indicates that caching is enabled.
		/// </summary>
		EnableCaching = 2,
		/// <summary>
		/// Indicates the creation context should inherit cached items from the outer context.
		/// </summary>
		InheritCache = EnableCaching | 4,
		/// <summary>
		/// Indicates that the context should inherit its scope from an outer scope if one exists.
		/// </summary>
		InheritScope = 8,
	}

	/// <summary>
	/// Kinds of creation events.
	/// </summary>
	public enum CreationEventKind
	{
		/// <summary>
		/// Indicates the factory created the instance (or caused to be created).
		/// </summary>
		Created = 0,
		/// <summary>
		/// Indicates the factory invoked an initializer for the instance.
		/// </summary>
		Initialized = 1,
		/// <summary>
		/// Indicates the factory copy-constructed an instance based on another instance.
		/// </summary>
		Copied = 2,
		/// <summary>
		/// Indicates the factory cached the instance.
		/// </summary>
		Cached = 3,
		/// <summary>
		/// Indicates the factory reissued the instance.
		/// </summary>
		Reissued = 4,
		/// <summary>
		/// Indicates the factory ducktyped an instance of another type.
		/// </summary>
		DuckType = 99,
	}
}
