using System;
using System.Collections;
using System.Collections.Generic;
using GD.SwitchHandler.VendorSwitchModels;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GDModel = GD.SwitchHandler.HidModels;

namespace GD.SwitchHandler {

    public interface ISwitchFactory
    {
        event GDModel.StateChangedHandler? StateChanged;
        int SwitchCount { get; }
        GDModel.Button Button(string id, string? name);
        GDModel.Button Button(SwitchDefinition def);
        GDModel.Switch Switch(string id);
        GDModel.Switch Switch(string id, string? name);
        GDModel.Switch Switch(string id, string? name, int? initialState);
        GDModel.Switch Switch(SwitchDefinition def);
        GDModel.Toggle? Toggle(string id, string? name
            , string ida, string idb, int? initialState);
        GDModel.Toggle? Toggle(SwitchDefinition def);

        void LoadDevice(int? vendorId, int? productId);
        void Update(string id, int state);
        void Update(DeviceItemInputParser inputParser);
    }

    public class SwitchFactory : ISwitchFactory
    {
        private readonly ILogger _logger;
        private readonly Hashtable _switches = new Hashtable();

        public event GDModel.StateChangedHandler? StateChanged;

        public SwitchFactory(ILogger logger)
        {
            _logger = logger;
        }

        public int SwitchCount => _switches.Count;

#region SwitchCreators

        public GDModel.Button Button(string id, string? name)
        {
            var b = new GDModel.Button(id, name);
            b.StateChanged += s_StateChanged;
            _switches[b.Id] = b;

            return b;
        }
        
        public GDModel.Button Button(SwitchDefinition def)
        {
            return Button(def.Id, def.Name);
        }

        public GDModel.Switch Switch(string id)
        {
            var s = new GDModel.Switch(id);
            s.StateChanged += s_StateChanged;
            _switches[s.Id] = s;

            return s;
        }

        /// <summary>
        /// Creates a <type>GD.SwitchHandler.HidModels.Switch</type> switch.
        /// </summary>
        /// <param name="id">The ID of the switch.</param>
        /// <returns>The newly created switch.</returns>
        public GDModel.Switch Switch(string id, string? name)
        {
            var s = new GDModel.Switch(id, name);
            s.StateChanged += s_StateChanged;
            _switches[s.Id] = s;

            return s;
        }

        /// <summary>
        /// Creates a <type>GD.SwitchHandler.HidModels.Switch</type> switch with a specific state.
        /// </summary>
        /// <param name="id">The ID of the switch.</param>
        /// <param name="initialState">An <type>int</type> value representing the initial state of switch. For basic switches, this is 0 or 1, but radials, for instance may have any number along the dial.</param>
        /// <returns>The newly created switch.</returns>
        public GDModel.Switch Switch(string id, string? name, int? initialState)
        {
            var s = new GDModel.Switch(id, name, initialState);
            s.StateChanged += s_StateChanged;
            _switches[s.Id] = s;

            return s;
        }
        
        public GDModel.Switch Switch(SwitchDefinition def)
        {
            return Switch(def.Id, def.Name, def.InitialState);
        }

        /// <summary>
        /// Creates a <type>GD.SwitchHandler.HidModels.Toggle</type> switch.
        /// </summary>
        /// <param name="id">The ID of the toggle.</param>
        /// <param name="ida">The ID of the underlying HID switch when the toggle is in first position. Represented by a state of -1.</param>
        /// <param name="idb">The ID of the underlying HID switch when the toggle is in second position. Represented by a state of 1.</param>
        /// <param name="initialState">May only be -1 or 1 which represents first position and second position of the toggle.</param>
        /// <returns>The newly created toggle.</returns>
        public GDModel.Toggle? Toggle(string id, string? name
            , string ida, string idb, int? initialState)
        {
            if(string.IsNullOrEmpty(ida))
            {
                _logger.LogError($"Ida must be set for a toggle. Reference Toggle definition {id}.");
                return null;
            }
            if(string.IsNullOrEmpty(idb))
            {
                _logger.LogError($"Idb must be set for a toggle. Reference Toggle definition {id}.");
                return null;
            }

            if(initialState is not -1 or 1)
            {
                _logger.LogError($"Toggle state may only be -1 or 1. Reference Toggle definition {id}.");
                return null;
            }                
            
            var a = Switch(ida, $"{id}-ida", (initialState == -1) ? 1 : 0);
            var b = Switch(idb, $"{id}-idb", (initialState == 1) ? 1 : 0);

            var t = new GDModel.Toggle(id, name, a, b);
            t.StateChanged += s_StateChanged;
            _switches[t.Id] = t;

            return t;
        }

        public GDModel.Toggle? Toggle(SwitchDefinition def)
        {
            if(string.IsNullOrEmpty(def.Ida))
            {
                _logger.LogError($"Ida must be set for a toggle. Reference Toggle definition {def.Id}.");
                return null;
            }
            if(string.IsNullOrEmpty(def.Idb))
            {
                _logger.LogError($"Idb must be set for a toggle. Reference Toggle definition {def.Id}.");
                return null;
            }
            return Toggle(def.Id, def.Name, def.Ida, def.Idb, def.InitialState );
        }

#endregion

        public void LoadDevice(int? vendorId, int? productId)
        {
            // load from json
            var filePath = $"C:\\src\\gd-switch-handler\\GD.SwitchHandler\\{vendorId}-{productId}.json";
            if(!File.Exists(filePath))
            {
                _logger.LogError($"Unable to find file {filePath}.");
                return;
            }
            var json = File.ReadAllText(filePath);
            var vendorSwitchMap = JsonConvert.DeserializeObject<VendorSwitchModels.VendorSwitchMap>(json);
            if(vendorSwitchMap == null)
            {
                _logger.LogError($"Unable to deserialize file {filePath}.");
                return;                
            }

            foreach(var def in vendorSwitchMap.SwitchMap)
            {
                switch (def.Type.ToLower())
                {
                    case("button"):
                        Button(def);
                        break;
                    case("switch"):
                        Switch(def);
                        break;
                    case("toggle"):
                        Toggle(def);
                        break;         
                }
            }
        }

        /// <summary>
        /// Updates the state of a given switch.
        /// </summary>
        /// <param name="id">The id of the switch to update.</param>
        /// <param name="state">The desired state to update the switch with.</param>
        public void Update(string id, int state)
        {
            if(_switches.ContainsKey(id)) 
            {
                var s = (_switches[id] as GDModel.Switch);
                if(s != null)
                    s.State = state;
            }                
        }

        public void Update(DeviceItemInputParser inputParser)
        {
            int changedIndex = inputParser.GetNextChangedIndex();
            var previousDataValue = inputParser.GetPreviousValue(changedIndex);
            var dataValue = inputParser.GetValue(changedIndex);

            _logger.LogDebug(string.Format("  {0}: {1} -> {2}",
                                (Usage)dataValue.Usages.FirstOrDefault(), previousDataValue.GetPhysicalValue(), dataValue.GetPhysicalValue()));

            var id = Convert.ToString(dataValue.Usages.FirstOrDefault());
            var value = Convert.ToInt32(dataValue.GetPhysicalValue());

            Update(id, value);            
        }

        private void s_StateChanged(GDModel.Switch sender, GDModel.SwitchStateChangedEventArgs args)
        {
            StateChanged?.Invoke(sender, args);
        }

    }
}