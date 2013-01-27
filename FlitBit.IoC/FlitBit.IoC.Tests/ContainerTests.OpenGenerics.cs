#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.IoC.Tests
{
	public interface IOpenGeneric<T,U>
	{
		Type TypeofT { get; }
		Type TypeofU { get; }
	}

	public class OpenGeneric<T,U> : IOpenGeneric<T,U>
	{
		public Type TypeofT { get { return typeof(T); } }
		public Type TypeofU { get { return typeof(U); } }
	}
		
	public partial class ContainerTests
	{
		[TestMethod]
		public void RegistrationCanBeMadeForOpenGenerics()
		{
			var root = Container.Root;
			root.ForGenericType(typeof(IOpenGeneric<,>))
				.Register(typeof(OpenGeneric<,>))
				.End();

			using (var c = Create.NewContainer())
			{
				Assert.IsNotNull(c);

				var a = c.New<IOpenGeneric<A, B>>();
				Assert.IsInstanceOfType(a, typeof(IOpenGeneric<A, B>));
				Assert.IsInstanceOfType(a, typeof(OpenGeneric<A, B>));
			}
		}
	}
}
