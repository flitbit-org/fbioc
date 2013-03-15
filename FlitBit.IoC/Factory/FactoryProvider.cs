using FlitBit.Core.Factory;

namespace FlitBit.IoC.Factory
{
	/// <summary>
	///   Factory provider implementation that returns the current IContainer.
	/// </summary>
	internal class FactoryProvider : IFactoryProvider
	{
		#region IFactoryProvider Members

		public IFactory GetFactory() { return Container.Current; }

		#endregion
	}
}