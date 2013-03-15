#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.IoC
{
	/// <summary>
	///   Enumeration of scope behaviors
	/// </summary>
	[Flags]
	public enum ScopeBehavior
	{
		/// <summary>
		///   Default == InstancePerRequest
		/// </summary>
		Default = 0,

		/// <summary>
		///   Indicates an instance is created for each
		///   container request. (Default)
		/// </summary>
		InstancePerRequest = 0,

		/// <summary>
		///   Indicates that specialization is no longer allowed.
		///   Pertains to the current container as well as any subsequent container scopes.
		/// </summary>
		SpecializationDisallowed = 1,

		/// <summary>
		///   Indicates an instance is created for each
		///   lifetime scope.
		/// </summary>
		InstancePerScope = 1 << 1,

		/// <summary>
		///   Indicates an instance is only created once
		///   within the shared-scope of a root container.
		/// </summary>
		Singleton = 1 << 2,

		/// <summary>
		///   Indicates an instances is a singleton and cannot be overriden
		///   once defined.
		/// </summary>
		LockedSingleton = Singleton | SpecializationDisallowed,
	}

	/// <summary>
	///   Enum of param kinds
	/// </summary>
	[Flags]
	public enum ParamKind
	{
		/// <summary>
		///   Indicates user supplied.
		/// </summary>
		UserSupplied = 0,

		/// <summary>
		///   Indicates container supplied.
		/// </summary>
		ContainerSupplied = 1,

		/// <summary>
		///   Indicates declared as the default.
		/// </summary>
		DeclaredDefault = 1 << 1,

		/// <summary>
		///   Indicates container supplied default
		/// </summary>
		ContainerDefault = 1 << 2,

		/// <summary>
		///   Indicates the param is named
		/// </summary>
		Named = 1 << 3,

		/// <summary>
		///   Indicates the param is default, named, and container supplied
		/// </summary>
		DefaultNamed = ContainerDefault | Named,

		/// <summary>
		///   Indicates the param is missing
		/// </summary>
		Missing = 0x40000000,
	}

	/// <summary>
	///   Options for creation contexts.
	/// </summary>
	[Flags]
	public enum CreationContextOptions
	{
		/// <summary>
		///   No options.
		/// </summary>
		None = 0,

		/// <summary>
		///   Indicates the creation context should track instances.
		/// </summary>
		InstanceTracking = 1,

		/// <summary>
		///   Indicates that caching is enabled.
		/// </summary>
		EnableCaching = 2,

		/// <summary>
		///   Indicates the creation context should inherit cached items from the outer context.
		/// </summary>
		InheritCache = EnableCaching | 4,

		/// <summary>
		///   Indicates that the context should inherit its scope from an outer scope if one exists.
		/// </summary>
		InheritScope = 8,
	}

	/// <summary>
	///   Kinds of creation events.
	/// </summary>
	public enum CreationEventKind
	{
		/// <summary>
		///   Indicates the factory created the instance (or caused to be created).
		/// </summary>
		Created = 0,

		/// <summary>
		///   Indicates the factory invoked an initializer for the instance.
		/// </summary>
		Initialized = 1,

		/// <summary>
		///   Indicates the factory copy-constructed an instance based on another instance.
		/// </summary>
		Copied = 2,

		/// <summary>
		///   Indicates the factory cached the instance.
		/// </summary>
		Cached = 3,

		/// <summary>
		///   Indicates the factory reissued the instance.
		/// </summary>
		Reissued = 4,

		/// <summary>
		///   Indicates the factory ducktyped an instance of another type.
		/// </summary>
		DuckType = 99,
	}
}