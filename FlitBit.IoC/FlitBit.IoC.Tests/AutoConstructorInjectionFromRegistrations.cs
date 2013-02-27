using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.IoC.Tests
{
	[TestClass]
	public class AutoConstructorInjectionFromRegistrations
	{
		public interface A
		{
			string AlphaString { get; set; }
		}

		public class Beta : A
		{
			public string AlphaString { get; set; }

			public Beta() { }

			public Beta(string alphaString)
			{
				AlphaString = alphaString;
			}
		}

		public interface E
		{
			int EchoInt { get; set; }
		}

		public class Foxtrot : E
		{
			public int EchoInt { get; set; }
		}

		public interface I
		{
			A Alpha { get; set; }
			string IndiaString { get; set; }
		}

		public class Juliett : I
		{
			public A Alpha { get; set; }

			public string IndiaString { get; set; }

			public Juliett(A alpha)
			{
				Alpha = alpha;
			}
		}

		public interface O
		{
			I India { get; set; }
		}

		public class Papa : O
		{
			public I India { get; set; }

			public Papa(I india)
			{
				India = india;
			}
		}

		public interface U
		{
			O Oscar { get; set; }
			E Echo { get; set; }
		}

		public class Victor : U
		{
			public O Oscar { get; set; }

			public E Echo { get; set; }

			public Victor(O oscar, E echo)
			{
				Oscar = oscar;
				Echo = echo;
			}
		}

		[TestMethod]
		public void Simple_Single_Argument_Constructor_Gets_Auto_Injected()
		{
			using (var c = Create.NewContainer())
			{
				c.ForType<A>().Register<Beta>().End();
				c.ForType<I>().Register<Juliett>().End();

				var india = Create.New<I>();
				Assert.IsNotNull(india);
				Assert.IsNotNull(india.Alpha);
			}
		}

		[TestMethod]
		public void Simple_Single_Argument_Constructor_Stack_Gets_Auto_Injected()
		{
			using (var c = Create.NewContainer())
			{
				c.ForType<A>().Register<Beta>().End();
				c.ForType<I>().Register<Juliett>().End();
				c.ForType<O>().Register<Papa>().End();

				var oscar = Create.New<O>();
				Assert.IsNotNull(oscar);
				Assert.IsNotNull(oscar.India);
				Assert.IsNotNull(oscar.India.Alpha);
			}
		}

		[TestMethod]
		public void Multiple_Argument_Constructor_Stack_Gets_Auto_Injected()
		{
			using (var c = Create.NewContainer())
			{
				c.ForType<A>().Register<Beta>().End();
				c.ForType<E>().Register<Foxtrot>().End();
				c.ForType<I>().Register<Juliett>().End();
				c.ForType<O>().Register<Papa>().End();
				c.ForType<U>().Register<Victor>().End();

				var uniform = Create.New<U>();
				Assert.IsNotNull(uniform);
				Assert.IsNotNull(uniform.Echo);
				Assert.IsNotNull(uniform.Oscar);
				Assert.IsNotNull(uniform.Oscar.India);
				Assert.IsNotNull(uniform.Oscar.India.Alpha);
			}
		}

		[TestMethod]
		public void Multiple_Argument_Constructor_Stack_Gets_Auto_Injected_With_Defaults()
		{
			using (var c = Create.NewContainer())
			{
				c.ForType<A>()
					.Register<Beta>(Param.Value("AlphaBeta"))
					.End();
				c.ForType<E>().Register<Foxtrot>().End();
				c.ForType<I>().Register<Juliett>().End();
				c.ForType<O>().Register<Papa>().End();
				c.ForType<U>().Register<Victor>().End();

				var uniform = Create.New<U>();
				Assert.IsNotNull(uniform);
				Assert.IsNotNull(uniform.Echo);
				Assert.IsNotNull(uniform.Oscar);
				Assert.IsNotNull(uniform.Oscar.India);
				Assert.IsNotNull(uniform.Oscar.India.Alpha);
				Assert.IsNotNull(uniform.Oscar.India.Alpha.AlphaString);
				Assert.AreEqual("AlphaBeta", uniform.Oscar.India.Alpha.AlphaString);
			}
		}

		[TestMethod]
		public void Multiple_Argument_Constructor_Stack_Fails_Auto_Injection()
		{
			using (var c = Create.NewContainer())
			{
				c.ForType<A>().Register<Beta>().End();
				c.ForType<E>().Register<Foxtrot>().End();
				//c.ForType<I>().Register<Juliett>().End();
				c.ForType<O>().Register<Papa>().End();
				c.ForType<U>().Register<Victor>().End();

				try
				{
					var uniform = Create.New<U>();
					Assert.Fail("Expected ContainerException");
				}
				catch (Exception ce)
				{
					Assert.IsInstanceOfType(ce, typeof(ContainerException));					
				} 
			}
		}
	}
}
