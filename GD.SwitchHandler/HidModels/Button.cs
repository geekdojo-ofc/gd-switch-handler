
namespace GD.SwitchHandler.HidModels {
    public class Button : Switch
    {
        internal Button(string id) : base(id) {}

        internal Button(string id, string? name) : base(id, name) {}

        public override int State
        {
            get => base.State;
            
            internal set {
                if(value == 1)
                    base.State = (base.State == 0) ? 1 : 0;
            }
        }
    }
}