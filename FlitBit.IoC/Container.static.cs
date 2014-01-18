#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
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


    internal class ContainerContextFlowProvider : IContextFlowProvider
    {
      static readonly Lazy<ContainerContextFlowProvider> Provider =
        new Lazy<ContainerContextFlowProvider>(CreateAndRegisterContextFlowProvider, LazyThreadSafetyMode.ExecutionAndPublication);

      static ContainerContextFlowProvider CreateAndRegisterContextFlowProvider()
      {
        var res = new ContainerContextFlowProvider();
        ContextFlow.RegisterProvider(res);
        return res;
      }

      [ThreadStatic]
      static Stack<IContainer> __scopes;

      public ContainerContextFlowProvider()
      {
        this.ContextKey = Guid.NewGuid();
      }

      public Guid ContextKey
      {
        get;
        private set;
      }

      public object Capture()
      {
        var top = Peek();
        if (top != null)
        {
          return top.ShareContainer();
        }
        return null;
      }

      public void Attach(ContextFlow context, object captureKey)
      {
        var scope = (captureKey as IContainer);
        if (scope != null)
        {
          if (__scopes == null)
          {
            __scopes = new Stack<IContainer>();
          }
          if (__scopes.Count > 0)
          {
            ReportAndClearOrphanedScopes(__scopes);
          }
          __scopes.Push(scope);
        }
      }

      private void ReportAndClearOrphanedScopes(Stack<IContainer> scopes)
      {
        scopes.Clear();
      }

      public void Detach(ContextFlow context, object captureKey)
      {
        var scope = (captureKey as IContainer);
        if (scope != null)
        {
          scope.Dispose();
        }
      }

      internal static void Push(IContainer scope)
      {
        var dummy = Provider.Value;
        if (__scopes == null)
        {
          __scopes = new Stack<IContainer>();
        }
        __scopes.Push(scope);
      }

      internal static bool TryPop(IContainer top)
      {
        if (__scopes != null && __scopes.Count > 0)
        {
          if (ReferenceEquals(__scopes.Peek(), top))
          {
            __scopes.Pop();
            return true;
          }
        }
        return false;
      }

      internal static IContainer Pop()
      {
        if (__scopes != null && __scopes.Count > 0)
        {
          return __scopes.Pop();
        }
        return default(IContainer);
      }


      internal static IContainer Peek()
      {
        if (__scopes != null && __scopes.Count > 0)
        {
          return __scopes.Peek();
        }
        return default(IContainer);
      }
    }

		/// <summary>
		///   Gets the container assigned to the current thread.
		/// </summary>
		public static IContainer Current
		{
			get
			{
				Contract.Ensures(Contract.Result<IContainer>() != null);

			  var res = ContainerContextFlowProvider.Peek();
				return res ?? Root;
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
					WireupCoordinator.SelfConfigure();
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