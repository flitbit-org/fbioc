using FlitBit.Core;
using FlitBit.IoC;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;

[assembly: Wireup(typeof(WireupThisAssembly))]

namespace FlitBit.IoC
{
	/// <summary>
	///   Wires up this assembly.
	/// </summary>
	public sealed class WireupThisAssembly : IWireupCommand
	{
		#region IWireupCommand Members

		/// <summary>
		///   Wires up this assembly.
		/// </summary>
		/// <param name="coordinator"></param>
		public void Execute(IWireupCoordinator coordinator) { FactoryProvider.SetFactoryProvider(new Factory.FactoryProvider()); }

		#endregion
	}
}