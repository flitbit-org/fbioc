using System;
using System.Threading;
using FlitBit.Wireup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.IoC.Tests
{
	[TestClass]
	public class CopyConstructTests
	{
		[TestCleanup]
		public void Cleanup()
		{
			var report = WireupCoordinator.Instance.ReportWireupHistory();
			TestContext.WriteLine("---------- Wireup Report ----------");
			TestContext.WriteLine(report);
		}

		public TestContext TestContext { get; set; }

		[TestMethod]
		public void Container_CanCopyConstructA2A()
		{
			var rand = new Random();

			using (var container = Create.NewContainer())
			{
				var a = container.New<A>();
				Assert.IsNotNull(a);
				a.Name = String.Concat("My name is random ", rand.Next().ToString("X"));
				a.When = DateTime.Now;

				var aa = container.New<A>();
				Assert.IsNotNull(aa);
				Assert.AreNotSame(a, aa);

				// No type inference...
				var aaa = container.NewCopy<A, A>(a);
				Assert.IsNotNull(aaa);
				Assert.AreNotSame(a, aaa);
				Assert.AreEqual(a.Name, aaa.Name);
				Assert.AreEqual(a.When, aaa.When);

				// Type inference...
				aaa = container.NewInit<A>().Init(a);
				Assert.IsNotNull(aaa);
				Assert.AreNotSame(a, aaa);
				Assert.AreEqual(a.Name, aaa.Name);
				Assert.AreEqual(a.When, aaa.When);
			}
		}

		[TestMethod]
		public void Container_CanCopyConstructB2AandB2C()
		{
			var rand = new Random();

			using (var container = Create.NewContainer())
			{
				var b = container.New<B>();
				Assert.IsNotNull(b);
				b.Name = String.Concat("My name is random ", rand.Next().ToString("X"));
				b.When = DateTime.Now;

				var aa = container.New<A>();
				Assert.IsNotNull(aa);
				Assert.AreNotSame(b, aa);

				var aaa = container.NewCopy<A, B>(b);
				Assert.IsNotNull(aaa);
				Assert.AreNotSame(b, aaa);
				Assert.AreEqual(b.Name, aaa.Name);
				Assert.AreEqual(b.When, aaa.When);

				var c = container.NewInit<C>().Init(b);
				Assert.IsNotNull(c);
				Assert.AreNotSame(b, c);
				Assert.AreEqual(b.Name, c.Name);
				Assert.AreEqual(b.When, c.When);
				Assert.AreEqual(b.InstanceCount, c.InstanceCount);
			}
		}

		public class A
		{
			public string Name { get; set; }
			public DateTime When { get; set; }
		}

		public class B : A
		{
			static int __count;
			public B() { this.InstanceCount = Interlocked.Increment(ref __count); }
			public int InstanceCount { get; set; }
		}

		public class C : B
		{}
	}
}