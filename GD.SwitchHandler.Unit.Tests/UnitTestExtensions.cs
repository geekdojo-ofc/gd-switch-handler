using Microsoft.Extensions.Logging;

namespace GD.SwitchHandler.Unit.Tests
{
    public static class UnitTestExtensions
    {
        public static SwitchFactory GetSwitchFactory()
        {
            return new SwitchFactory(new Moq.Mock<ILogger>().Object);            
        }
    }
}