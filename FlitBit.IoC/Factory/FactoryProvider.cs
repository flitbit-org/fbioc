#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

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