namespace GD.SwitchHandler.Unit.Tests;

[TestClass]
public class ToggleUnitTest
{

    [TestMethod]
    public void CanCreateToggle()
    {
        var factory = UnitTestExtensions.GetSwitchFactory();
        var toggle = factory.Toggle("test", "test", "sw1", "sw2", -1);

        Assert.AreEqual("test", toggle.Id);
        Assert.AreEqual(-1, toggle.State);
    }

    [TestMethod]
    public void CanUpdateToggleState()
    {
        var factory = UnitTestExtensions.GetSwitchFactory();        
        var toggle = factory.Toggle("test", "test", "swa", "swb", -1);
        toggle.StateChanged += (ss, e) => {
            Assert.AreEqual(toggle.Id, ss.Id);
            Assert.AreEqual(e.OldState, -1);
        };

        var timesCalled = 0;
        factory.StateChanged += (ss, e) => {
            if(ss.Id == "test")
                Assert.AreEqual(-1, e.OldState);
            if(ss.Id == "swa")
            {
                Assert.AreEqual(0, ss.State);
                Assert.AreEqual(1, e.OldState);
            }                
            if(ss.Id == "swb")
            {
                Assert.AreEqual(1, ss.State);
                Assert.AreEqual(0, e.OldState);
            }

            timesCalled++;
        };

        factory.Update(toggle.Id, 1);
        Assert.AreEqual(1, toggle.State);
        Assert.AreEqual(3, timesCalled);
    }
}