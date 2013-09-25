using FlitBit.Core;
using FlitBit.Wireup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.IoC.Tests
{
    public class Person : IPerson
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public interface IPerson
    {
        string Name { get; set; }
        int Age { get; set; }
    }

    public interface IZombie : IPerson
    {
        bool IsDead { get; set; }
    }

    [TestClass]
    public class ContainerCreateUntypedTests : AbstractTests
    {
        [TestMethod]
        public void For_A_Registered_Type_Container_Should_Create_Untyped()
        {
            using (var container = Create.SharedOrNewContainer())
            {
                container
                    .ForType<IPerson>()
                    .Register<Person>()
                    .End();

                var personInterface = typeof(IPerson);
                var personClass = typeof(Person);
                var zombieInterface = typeof(IZombie);

                Assert.IsTrue(container.CanConstruct(personInterface));
                Assert.IsTrue(container.CanConstruct<IPerson>());

                Assert.IsTrue(FactoryProvider.Factory.CanConstruct(personInterface));
                Assert.IsTrue(FactoryProvider.Factory.CanConstruct<IPerson>());

                Assert.IsTrue(container.CanConstruct(personClass));
                Assert.IsTrue(container.CanConstruct<Person>());

                Assert.IsTrue(FactoryProvider.Factory.CanConstruct(personClass));
                Assert.IsTrue(FactoryProvider.Factory.CanConstruct<Person>());

                Assert.IsFalse(container.CanConstruct(zombieInterface));
                Assert.IsFalse(container.CanConstruct<IZombie>());

                Assert.IsFalse(FactoryProvider.Factory.CanConstruct(zombieInterface));
                Assert.IsFalse(FactoryProvider.Factory.CanConstruct<IZombie>());

                var iPersonFromNew = container.New<IPerson>();
                Assert.IsNotNull(iPersonFromNew);

                var iPersonFromUntyped = container.NewUntyped(personInterface);
                var personFromUntyped = container.NewUntyped(personClass);

                Assert.IsInstanceOfType(iPersonFromUntyped, personInterface);
                Assert.IsInstanceOfType(personFromUntyped, personClass);
            }
        }
    }

    public abstract class AbstractTests
    {
        protected AbstractTests()
        {
            WireupCoordinator.SelfConfigure();
        }
    }
}
