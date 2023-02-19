using System;

namespace GD.SwitchHandler.HidModels {
    public class Toggle : Switch
    {
        protected readonly Switch _a;
        protected readonly Switch _b;

        internal Toggle(string id, string? name, Switch a, Switch b) 
            : base(id, name)
        {
            _a = a;
            _b = b;

            if(_a.State == _b.State)
                throw new InvalidOperationException("The underlying switches in a toggle may not have the same state.");
            
            _state = (_a.State == 1) ? -1 : 1;
            _a.StateChanged += s_StateChanged;
            _b.StateChanged += s_StateChanged;
        }

        public override int State 
        { 
            get 
            {
                return base.State;
            }
                        
            internal set {
                if(base.State == value) return;

                switch(value)
                {
                    case (-1):
                        _a.State = 1;
                        _b.State = 0;
                        break;
                    case (1):
                        _a.State = 0;
                        _b.State = 1;
                        break;
                    default:
                        throw new InvalidOperationException("A toggle's state may only be -1 or 1.");
                }
            }
        }

        protected virtual void s_StateChanged(Switch sender, SwitchStateChangedEventArgs args)
        {
            if(sender == _a && sender.State == 1) base.State = -1;
            if(sender == _b && sender.State == 1) base.State = 1;
        }

    }
}