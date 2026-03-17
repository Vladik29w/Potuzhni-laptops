using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LaptopServer.Entities
{
    public class LaptopEntity
    {
        [MaxLength(256)]
        public required string Id { get; init; }//це артикул
        [MaxLength(512)]
        public required string Name { get; set; }
        [Precision(18, 2)]
        [Range(typeof(decimal), "0", "9999999.99")]
        public required decimal Price { get; set; }        
        [MaxLength(512)]
        public string? Img { get; set; }
        [MaxLength(128)]
        public string? CPU { get; set; }
        [Range(0, 512)]
        public int RAM { get; set; } = 0;
        [MaxLength(128)]
        public string? GPU { get; set; }
    }
}
