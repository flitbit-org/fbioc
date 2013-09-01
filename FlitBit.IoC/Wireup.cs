using FlitBit.Core;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;

[assembly: Wireup(typeof(FlitBit.IoC.AssemblyWireup))]

namespace FlitBit.IoC
{
	/// <summary>
	///   Wires up this assembly.
	/// </summary>
	public sealed class AssemblyWireup : IWireupCommand
	{

		/// <summary>
		///   Wires up this assembly.
		/// </summary>
		/// <param name="coordinator"></param>
		public void Execute(IWireupCoordinator coordinator)
		{
			FactoryProvider.SetFactoryProvider(new Factory.FactoryProvider());
		}
	}
}