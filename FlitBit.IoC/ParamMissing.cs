#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using FlitBit.Core;

namespace FlitBit.IoC
{
	internal sealed class ParamMissing : Param
	{
		readonly string _name;
		readonly int _position;

		public ParamMissing(string name, int position, Type type)
			: base(ParamKind.Missing, type)
		{
			_name = name;
			_position = position;
		}

		public override object GetValue(IContainer container)
		{
			throw new MissingParameterException(String.Concat("Required parameter missing: {Name: '", _name,
																												"', Position: ", _position, ", Type: '", TypeofValue.GetReadableFullName(), "'}"));
		}
	}
}