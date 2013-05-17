using FlitBit.Core;
using FlitBit.Wireup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.IoC.Tests
{
	[TestClass]
	public class CreateNewInitTests
	{
		[TestInitialize]
		public void Init()
		{
			WireupCoordinator.SelfConfigure();
			FactoryProvider.Factory.RegisterImplementationType<IHuman, Human>();
			FactoryProvider.Factory.RegisterImplementationType<ISneakyHuman, SneakyHuman>();
			FactoryProvider.Factory.RegisterImplementationType<INinja, Ninja>();
		}

		[TestMethod]
		public void Create_NewInit_With_Simple_Human_Model_Should_Initialize_Properly()
		{
			const string name = "Billy Maddison";
			const int age = 30;
			const float weight = 170.5f;

			var human = Create.NewInit<IHuman>().Init(new
																										{
																											Name = name,
																											Age = age,
																											Weight = weight
																										});
			Assert.AreEqual(name, human.Name);
			Assert.AreEqual(age, human.Age);
			Assert.AreEqual(weight, human.Weight);
		}

		[TestMethod]
		public void Create_NewInit_With_SneakyHuman_Model_Should_Initialize_Properly()
		{
			const string name = "Billy Maddison";
			const int age = 30;
			const float weight = 170.5f;
			const bool isSneaky = true;

			var sneakyHuman = Create.NewInit<ISneakyHuman>().Init(new
			{
				Name = name,
				Age = age,
				Weight = weight,
				IsSneaky = isSneaky
			});

			Assert.AreEqual(name, sneakyHuman.Name);
			Assert.AreEqual(age, sneakyHuman.Age);
			Assert.AreEqual(weight, sneakyHuman.Weight);
			Assert.IsTrue(sneakyHuman.IsSneaky);
		}

		[TestMethod]
		public void Create_NewInit_With_Ninja_Model_Should_Initialize_Properly()
		{
			const string name = "The Great White Ninja";
			const int age = 25;
			const float weight = 400.432f;
			const bool isSneaky = true;
			const int rank = 1;

			var ninja = Create.NewInit<INinja>().Init(new
			{
				Name = name,
				Age = age,
				Weight = weight,
				IsSneaky = isSneaky,
				Rank = rank
			});

			Assert.AreEqual(name, ninja.Name);
			Assert.AreEqual(age, ninja.Age);
			Assert.AreEqual(weight, ninja.Weight);
			Assert.IsTrue(ninja.IsSneaky);
			Assert.AreEqual(rank, ninja.Rank);
		}
	}


	public interface IHuman
	{
		int Age { get; set; }
		string Name { get; set; }
		float Weight { get; set; }
	}

	public class Human : IHuman
	{
		#region IHuman Members

		public string Name { get; set; }
		public int Age { get; set; }
		public float Weight { get; set; }

		#endregion
	}

	public interface ISneakyHuman : IHuman
	{
		bool IsSneaky { get; set; }
	}

	public class SneakyHuman : Human, ISneakyHuman
	{
		#region ISneakyHuman Members

		public bool IsSneaky { get; set; }

		#endregion
	}

	public interface INinja : ISneakyHuman
	{
		int Rank { get; set; }
	}

	public class Ninja : SneakyHuman, INinja
	{
		#region INinja Members

		public int Rank { get; set; }

		#endregion
	}
}