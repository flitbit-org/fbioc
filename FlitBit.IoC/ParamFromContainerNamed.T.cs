#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

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