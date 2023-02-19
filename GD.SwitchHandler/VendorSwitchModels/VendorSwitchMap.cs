namespace GD.SwitchHandler.VendorSwitchModels
{
    public class VendorSwitchMap
    {
        public VendorSwitchMap(string vendor, string product
            , int? vendorId, int? productId)
        {  
            Vendor = vendor;
            Product = product;
            VendorId = vendorId;
            ProductId = productId;
            
            SwitchMap = new List<SwitchDefinition>();            
        }
        
        [Newtonsoft.Json.JsonConverter(typeof(HexStringJsonConverter))]
        public int? VendorId {get; set;}
        
        public string Vendor {get; set;}

        [Newtonsoft.Json.JsonConverter(typeof(HexStringJsonConverter))]
        public int? ProductId {get; set;}
        
        public string Product {get; set;}

        public List<SwitchDefinition> SwitchMap {get; set;}
    }
}