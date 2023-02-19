namespace GD.SwitchHandler.Unit.Tests;

[TestClass]
public class SwitchUnitTest
{

    [TestMethod]
    public void CanCreateSwitch()
    {
        var factory = UnitTestExtensions.GetSwitchFactory();
        var s = factory.Switch("test");

        Assert.AreEqual("test", s.Id);
        Assert.AreEqual(0, s.State);
    }
    
    [TestMethod]
    public void CanCreateSwitchWithInitialValue()
    {
        var factory = UnitTestExtensions.GetSwitchFactory();
        var s = factory.Switch("test", "test", 1);

        Assert.AreEqual("test", s.Id);
        Assert.AreEqual(1, s.State);
    }

    [TestMethod]
    public void CanUpdateSwitchState()
    {
        var factory = UnitTestExtensions.GetSwitchFactory();        
        var s = factory.Switch("test");
        s.StateChanged += (ss, e) => {
            Assert.AreEqual(s.Id, ss.Id);
            Assert.AreEqual(e.OldState, 0);
        };
        factory.StateChanged += (ss, e) => {
            Assert.AreEqual(s.Id, ss.Id);
            Assert.AreEqual(e.OldState, 0);
        };

        factory.Update(s.Id, 1);
        Assert.AreEqual(1, s.State);
    }
}