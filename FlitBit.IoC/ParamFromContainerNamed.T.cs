
namespace FlitBit.IoC
{
	internal sealed class ParamFromContainerNamed<T> : Param
	{
		private string RegistrationName { get; set; }

		public ParamFromContainerNamed(string registrationName)
			: base(ParamKind.ContainerSupplied, typeof(T))
		{
			RegistrationName = registrationName;
		}

		public override object GetValue(IContainer container)
		{
			return container.NewNamed<T>(RegistrationName);
		}
	}
}
