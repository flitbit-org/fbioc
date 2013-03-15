namespace FlitBit.IoC
{
	internal sealed class ParamFromContainerNamed<T> : Param
	{
		public ParamFromContainerNamed(string registrationName)
			: base(ParamKind.ContainerSupplied, typeof(T)) { RegistrationName = registrationName; }

		string RegistrationName { get; set; }

		public override object GetValue(IContainer container) { return container.NewNamed<T>(RegistrationName); }
	}
}