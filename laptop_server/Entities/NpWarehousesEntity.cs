using System.Text.Json.Serialization;

namespace LaptopServer.Entities
{
    public class NpWarehousesEntity
    {
        public string? Description { get; set; }
        public required string Ref { get; set; }
        public required string SettlementRef { get; set; }
        public string? TypeOfWarehouseRef { get; set; }
    }
}
