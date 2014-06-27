#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

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