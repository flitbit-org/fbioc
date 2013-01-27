﻿#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using FlitBit.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.IoC.Tests
{	
	[TestClass]
	public partial class ParamTests
	{
		[TestMethod]
		public void ParametersByName()
		{
			var pp = new Param[] {
				Param.Named("a", "a"),
				Param.Named("b", "b"),
				Param.Named("c", 3),
				Param.Named("d", 4d)			
				};

			Assert.AreEqual("a", pp.Named<string>("a", null));
			Assert.AreEqual(4d, pp.Named<double>("d", null));
			Assert.AreEqual(3, pp.Named<int>("c", null));
			Assert.AreEqual("b", pp.Named<string>("b", null));
		}

		[TestMethod]
		public void ParametersByType()
		{
			var pp = new Param[] {
				Param.Named("a", "a"),
				Param.Named("b", "b"),
				Param.Named("b", 3),
				Param.Named("d", 4d)			
				};

			var b = pp.First(p => p.TypeofValue == typeof(int));
			Assert.AreEqual(typeof(int), b.TypeofValue);
			Assert.AreEqual(3, b.GetValue(null));
			Assert.IsTrue(b.HasName);			
			Assert.AreEqual("b", b.Name);
		}

		[TestMethod]
		public void ContainerCanCreateInstanceWithConstructorParametersSupplied()
		{
			var args = new
			{
				AnInt = 1967,
				ADouble = 3.14,
				AnObject = new Object(),
				AnA = Create.New<A>(),
			};

			using (var scope = Create.NewContainer())
			{
				F f = scope.NewWithParams<F>(
						Param.Value(args.AnInt),
						Param.Value(args.ADouble),
						Param.Value(args.AnObject),
						Param.Value(args.AnA));

				Assert.IsNotNull(f);
				Assert.AreEqual(args.AnInt, f.AnInt);
				Assert.AreEqual(args.ADouble, f.ADouble);
				Assert.AreEqual(args.AnObject, f.AnObject);
				Assert.AreEqual(args.AnA, f.AnA);
			}
		}

		[TestMethod]
		public void ContainerCanCreateInstanceViaFactoryWithConstructorParametersSupplied()
		{
			var args = new
			{
				AnInt = 1967,
				ADouble = 3.14,
				AnObject = new Object(),
				AnA = Create.New<A>(),
			};

			using (var scope = Create.NewContainer())
			{
				scope.ForType<F>().Register((c, p) => new F(
					p.OfType<double>(c),
					p.At<object>(2, c),
					p.Named<A>("a", c)
					));

				var f = scope.NewWithParams<F>(
						Param.Value(args.AnInt),
						Param.Value(args.ADouble),
						Param.Value(args.AnObject),
						Param.Named("a", args.AnA));

				Assert.IsNotNull(f);
				Assert.AreNotEqual(args.AnInt, f.AnInt, "wasn't used by the factory");
				Assert.AreEqual(args.ADouble, f.ADouble);
				Assert.AreEqual(args.AnObject, f.AnObject);
				Assert.AreEqual(args.AnA, f.AnA);
			}
		}


		[TestMethod]
		public void ContainerCanCreateInstanceAndMatchAmongMultipleConstructors()
		{
			var args = new
			{
				AnInt = 1967,
				ADouble = 3.14,
				AnObject = new Object(),
				AnA = new A(),
			};

			using (var scope = Create.NewContainer())
			{
				F f = scope
					.NewWithParams<F>(
						Param.Value(args.ADouble),
						Param.Resolve<object>(),
						Param.Resolve<A>());

				Assert.IsNotNull(f);
				Assert.AreNotEqual(args.AnInt, f.AnInt);

				// The arguments supplied to the container came back in the instance...
				Assert.AreEqual(args.ADouble, f.ADouble);

				// The container created new instances of these...
				Assert.AreNotEqual(args.AnObject, f.AnObject);
				Assert.AreNotEqual(args.AnA, f.AnA);

				F ff = scope.NewWithParams<F>(
					Param.Value(args.AnInt),
					Param.Value(args.ADouble),
					Param.Value(args.AnObject),
					Param.Value(args.AnA)
					);

				Assert.IsNotNull(ff);
				Assert.AreNotEqual(f, ff);

				// The arguments supplied to the container came back in the instance...				
				Assert.AreEqual(args.AnInt, ff.AnInt);
				Assert.AreEqual(args.ADouble, ff.ADouble);
				Assert.AreEqual(args.AnObject, ff.AnObject);
				Assert.AreEqual(args.AnA, ff.AnA);
			}
		}
	}
}
