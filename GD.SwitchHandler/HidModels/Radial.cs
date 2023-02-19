
namespace GD.SwitchHandler.HidModels {
    public class Radial : Toggle
    {
        internal Radial(string id, string? name, Switch a, Switch b) 
            : base(id, name, a, b)
        {
        }

        protected override void s_StateChanged(Switch sender, SwitchStateChangedEventArgs args)
        {
            if(sender == _a && sender.State == 1) State = State--;
            if(sender == _b && sender.State == 1) State = State++;
        }

    }
}