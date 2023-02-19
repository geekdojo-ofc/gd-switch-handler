
using Microsoft.Extensions.Logging;

namespace GD.SwitchHandler.Unit.Tests
{

    [TestClass]
    public class SwitchFactoryUnitTest
    {
        [TestMethod]
        public void CanCreate()
        {
            Assert.IsInstanceOfType(new SwitchFactory(new Moq.Mock<ILogger>().Object), typeof(SwitchFactory));
        }

        [TestMethod]
        public void CanLoadSwitchesFromFile()
        {
            var factory = UnitTestExtensions.GetSwitchFactory();
            factory.LoadDevice(0x3344, 0x0259);
            Assert.AreEqual(24, factory.SwitchCount);
        }

        [TestMethod]
        public void CanUpdateButton()
        {
            var factory = UnitTestExtensions.GetSwitchFactory();
            factory.Button("1234", "test");
            var eventCalled = false;
            factory.StateChanged += (s, e) =>
            {
                Assert.AreEqual("1234", s.Id);
                Assert.AreEqual(1, s.State);
                Assert.AreEqual(0, e.OldState);
                eventCalled = true;
            };
            factory.Update("1234", 1);
            Assert.IsTrue(eventCalled);
        }
    }
}