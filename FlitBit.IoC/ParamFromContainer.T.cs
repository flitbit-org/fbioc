using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlitBit.IoC
{
	internal sealed class ParamFromContainer<T> : Param
	{
		public ParamFromContainer()
			: base(ParamKind.ContainerSupplied, typeof(T))
		{
		}

		public override object GetValue(IContainer container)
		{
			return container.New<T>();
		}
	}
}
