using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlitBit.IoC
{
	internal sealed class ParamValueNamed<T> : ParamValue<T>
	{
		public ParamValueNamed(ParamKind kind, string name, T value)
			: base(kind, value)
		{
			Name = name;
		}
	}
}
