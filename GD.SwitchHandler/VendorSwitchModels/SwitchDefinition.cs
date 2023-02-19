namespace GD.SwitchHandler.VendorSwitchModels
{
    public class SwitchDefinition
    {
        public SwitchDefinition(string id, string type
            , string? name, string? ida, string? idb, int? initialState)
        {
            Name = (string.IsNullOrEmpty(name)) ? id : name;
            Type = type;
            Id = id;
            Ida = ida;
            Idb = idb;
            InitialState = initialState;
        }

        public string Name {get; set;}
        public string Type {get;set;}
        public string Id {get; set;}
        public string? Ida {get; set;}
        public string? Idb {get; set;}
        public int? InitialState {get; set;}
    }
}