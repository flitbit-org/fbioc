using FlitBit.Core;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;
using System;

[assembly: Wireup(typeof(FlitBit.IoC.WireupThisAssembly))]

namespace FlitBit.IoC
{
	/// <summary>
	/// Wires up this assembly.
	/// </summary>
	public sealed class WireupThisAssembly : IWireupCommand
	{
		/// <summary>
		/// Wires up this assembly.
		/// </summary>
		/// <param name="coordinator"></param>
		public void Execute(IWireupCoordinator coordinator)
		{
			FactoryProvider.SetFactoryProvider(new FlitBit.IoC.Factory.FactoryProvider());
		}
	}
}
