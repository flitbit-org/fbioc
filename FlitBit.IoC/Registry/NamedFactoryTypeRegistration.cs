using System;
using System.Diagnostics.Contracts;

namespace FlitBit.IoC.Registry
{
	internal sealed class NamedFactoryTypeRegistration<T, TConcrete> : FactoryTypeRegistration<T, TConcrete>, INamedTypeRegistration<T>
		where TConcrete : T
	{
		internal NamedFactoryTypeRegistration(IContainer container, string name, Func<IContainer, Param[], TConcrete> factory)
			: base(container, factory)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentException>(name.Length > 0);

			IsNamed = true;
			this.Name = name;
		}

		#region INamedTypeRegistration<T> Members

		public string Name { get; private set; }

		#endregion
	}
}