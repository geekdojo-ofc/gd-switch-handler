using System;

namespace GD.SwitchHandler.HidModels {

    internal interface ISwitch
    {
        string Id { get; }
        int State { get; }
    }

    public class SwitchStateChangedEventArgs
    {
        public SwitchStateChangedEventArgs(int oldState)
        {
            OldState = oldState;
        }
        public int OldState { get; }        
    }

    public delegate void StateChangedHandler(Switch sender, SwitchStateChangedEventArgs args);

    public class Switch : ISwitch
    {
        public event StateChangedHandler? StateChanged;
        protected int _state = 0;

        
        internal Switch(string id)
        {
            Id = id;
            Name = id;
        }
        
        internal Switch(string id, string? name)
        {
            Id = id;
            Name = (name == null) ? id : name;
        }
    
        internal Switch(string id, string? name, int? initialState)
        {
            Id = id;
            Name = (name == null) ? id : name;
            if(initialState != null)
                _state = (int)initialState;
        }

        protected virtual void RaiseStateChangedEvent(int oldState)
        {
            StateChanged?.Invoke(this, new SwitchStateChangedEventArgs(oldState));
        }

        public string Name { get; private set;}

        public string Id { get; private set; }

        public virtual int State 
        { 
            get { return _state; } 
            internal set {
                
                var oldState = _state;
                _state = value;

                if (oldState != _state)
                    RaiseStateChangedEvent(oldState);
            }
        }

        public override bool Equals(Object? obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else {
                var s = (Switch)obj;
                return s.Id == this.Id;
            }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Id;
        }
    }
}