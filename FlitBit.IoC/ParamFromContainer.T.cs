namespace FlitBit.IoC
{
	internal sealed class ParamFromContainer<T> : Param
	{
		public ParamFromContainer()
			: base(ParamKind.ContainerSupplied, typeof(T)) { }

		public override object GetValue(IContainer container) { return container.New<T>(); }
	}
}