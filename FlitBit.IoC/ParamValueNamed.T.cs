#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

namespace FlitBit.IoC
{
	internal sealed class ParamValueNamed<T> : ParamValue<T>
	{
		public ParamValueNamed(ParamKind kind, string name, T value)
			: base(kind, value) { Name = name; }
	}
}