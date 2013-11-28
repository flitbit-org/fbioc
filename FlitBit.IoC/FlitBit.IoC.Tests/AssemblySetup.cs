using System;
using FlitBit.Emit;
using FlitBit.Wireup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.IoC.Tests
{
  [TestClass]
  public class AssemblySetup
  {
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext ctx)
    {
      RuntimeAssemblies.WriteDynamicAssemblyOnExit = true;
      WireupCoordinator.SelfConfigure();
    }

    [AssemblyCleanup]
    public static void AssemblyClean()
    {
      Console.WriteLine(WireupCoordinator.Instance.ReportWireupHistory());
    }
  }
}
