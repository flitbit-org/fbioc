#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using FlitBit.Core.Parallel;
using FlitBit.IoC.Containers;
using FlitBit.Wireup;

namespace FlitBit.IoC
{
	/// <summary>
	///   Utility class for working with containers.
	/// </summary>
	public static class Container
	{
		const string FlitBitLogicalRootContainer = "FlitBit_LogicalRoot_Container";
		static bool __initialized;

		static readonly Lazy<IRootContainer> LazyRoot = new Lazy<IRootContainer>(() => new RootContainer(),
																																					LazyThreadSafetyMode.ExecutionAndPublication);

		/// <summary>
		///   Gets the container assigned to the current thread.
		/// </summary>
		public static IContainer Current
		{
			get
			{
				Contract.Ensures(Contract.Result<IContainer>() != null);

				IContainer res;
				return (ContextFlow.TryPeek(out res)) ? res : Root;
			}
		}

		/// <summary>
		///   Gets the logical root container.
		/// </summary>
		public static IContainer LogicalRoot
		{
			get
			{
				var tenantid = CallContext.LogicalGetData(FlitBitLogicalRootContainer);
				if (tenantid != null)
				{
					return Root.ResolveTenantByID(tenantid);
				}
				return LazyRoot.Value;
			}
		}

		/// <summary>
		///   Gets the root container.
		/// </summary>
		public static IRootContainer Root
		{
			get
			{
				var root = LazyRoot.Value;
				if (!__initialized)
				{
					WireupCoordinator.Instance.WireupDependencies(Assembly.GetExecutingAssembly());
					__initialized = true;
				}
				return root;
			}
		}

		/// <summary>
		///   Identifies the current tenant's container as the logical root container.
		/// </summary>
		public static void IdentifyTenantAsLogicalRoot()
		{
			object tenantid;
			if (Root.TryResolveTenant(out tenantid))
			{
				CallContext.LogicalSetData(FlitBitLogicalRootContainer, tenantid);
			}
		}
	}
}