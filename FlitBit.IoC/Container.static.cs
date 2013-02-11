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
	/// Utility class for working with containers.
	/// </summary>
	public static class Container
	{
		static readonly string FlitBit_LogicalRoot_Container = "FlitBit_LogicalRoot_Container";
				
		/// <summary>
		/// Gets the container assigned to the current thread.
		/// </summary>
		public static IContainer Current
		{
			get
			{
				Contract.Ensures(Contract.Result<IContainer>() != null);

				IContainer res;
				return (ContextFlow.TryPeek<IContainer>(out res)) ? res : Root;
			}
		}
				
		/// <summary>
		/// Identifies the current tenant's container as the logical root container.
		/// </summary>
		public static void IdentifyTenantAsLogicalRoot()
		{
			object tenantid;
			if (Root.TryResolveTenant(out tenantid))
			{
				CallContext.LogicalSetData(FlitBit_LogicalRoot_Container, tenantid);
			}
		}

		/// <summary>
		/// Gets the logical root container.
		/// </summary>
		public static IContainer LogicalRoot
		{
			get
			{
				var tenantid = CallContext.LogicalGetData(FlitBit_LogicalRoot_Container);
				if (tenantid != null)
				{
					return Root.ResolveTenantByID(tenantid);
				}
				return __root.Value;
			}
		}

		/// <summary>
		/// Gets the root container.
		/// </summary>
		public static IRootContainer Root
		{
			get
			{
				var root = __root.Value;
				if (!__initialized)
				{
					WireupCoordinator.Instance.WireupDependencies(Assembly.GetExecutingAssembly());
					__initialized = true;
				}
				return root;
			}
		}

		static bool __initialized;
		static readonly Lazy<IRootContainer> __root = new Lazy<IRootContainer>(() => new RootContainer(), LazyThreadSafetyMode.ExecutionAndPublication);
	}
}
