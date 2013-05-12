#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using FlitBit.IoC;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;
using FlitBit.Wireup.Recording;

[assembly: ModuleWireupTask]

namespace FlitBit.IoC
{
	/// <summary>
	///   Wires this module.
	/// </summary>
	public class ModuleWireupTask : WireupTaskAttribute
	{
		/// <summary>
		///   Creates a new instance.
		/// </summary>
		public ModuleWireupTask()
			: base(WireupPhase.BeforeDependencies) { }

		/// <summary>
		///   Performs wireup.
		/// </summary>
		/// <param name="coordinator"></param>
		/// <param name="context"></param>
		protected override void PerformTask(IWireupCoordinator coordinator, WireupContext context)
		{
			// Attach the root container as a wireup observer...
			coordinator.RegisterObserver(Container.Root);
		}
	}
}