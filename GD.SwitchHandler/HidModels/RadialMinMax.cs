using System;

namespace GD.SwitchHandler.HidModels {
    public class RadialMinMax : Radial
    {
        private readonly int _min;
        private readonly int _max;

        internal RadialMinMax(string id, string? name, Switch a, Switch b, int min, int max) 
            : base(id, name, a, b)
        {
            _min = min;
            _max = max;
        }

        protected override void s_StateChanged(Switch sender, SwitchStateChangedEventArgs args)
        {
            if(sender == _a && sender.State == 1) 
            {
                if((State - 1) < _min)
                    throw new IndexOutOfRangeException("Attempting to set the Radial below minimums.");

                State--;
            }
            if(sender == _b && sender.State == 1)
            {
                if((State + 1) > _max)
                    throw new IndexOutOfRangeException("Attempting to set the Radial above maximums.");

                State++;
            }
        }

    }
}