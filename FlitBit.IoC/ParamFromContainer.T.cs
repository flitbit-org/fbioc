#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

namespace FlitBit.IoC
{
	internal sealed class ParamFromContainer<T> : Param
	{
		public ParamFromContainer()
			: base(ParamKind.ContainerSupplied, typeof(T)) { }

		public override object GetValue(IContainer container) { return container.New<T>(); }
	}
}