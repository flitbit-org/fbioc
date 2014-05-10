#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

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