#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;

namespace FlitBit.IoC.Registry
{
	internal sealed class NamedTypeRegistration<T, TConcrete> : TypeRegistration<T, TConcrete>, INamedTypeRegistration<T>
		where TConcrete : class, T
	{
		public NamedTypeRegistration(IContainer container, string name, Param[] parameters)
			: base(container, parameters)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentException>(name.Length > 0);
			
			base.IsNamed = true;
			this.Name = name;
		}

		#region INamedTypeRegistration<T> Members

		public string Name { get; private set; }

		#endregion
	}
}