#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using FlitBit.Wireup.Meta;

[assembly: FlitBit.IoC.ModuleWireupTask]

namespace FlitBit.IoC
{
	/// <summary>
	/// Wires this module.
	/// </summary>
	public class ModuleWireupTask : WireupTaskAttribute
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public ModuleWireupTask()
			: base(Wireup.WireupPhase.BeforeTasks)
		{
		}

		/// <summary>
		/// Performs wireup.
		/// </summary>
		/// <param name="coordinator"></param>
		protected override void PerformTask(Wireup.IWireupCoordinator coordinator)
		{
			// Attach the root container as a wireup observer...
			coordinator.RegisterObserver(Container.Root as IRootContainer);
		}
	}
}
