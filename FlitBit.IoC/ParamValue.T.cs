#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

namespace FlitBit.IoC
{
	internal class ParamValue<T> : Param
	{
		public ParamValue(ParamKind kind, T value)
			: base(kind, typeof(T)) { Value = value; }

		public T Value { get; private set; }
		public override object GetValue(IContainer container) { return Value; }
	}
}