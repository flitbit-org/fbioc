using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlitBit.IoC
{
	internal class ParamValue<T> : Param
	{
		public ParamValue(ParamKind kind, T value)
			: base(kind, typeof(T))
		{
			Value = value;
		}

		public override object GetValue(IContainer container)
		{
			return Value;
		}

		public T Value { get; private set; }
	}
}
