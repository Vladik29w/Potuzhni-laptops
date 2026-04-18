using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LaptopServer.Entities
{
    public class LaptopEntity
    {
        public Guid Id { get; set; }
        [MaxLength(64)]
        public required string Name { get; set; }
        [Precision(18, 2)]
        [Range(typeof(decimal), "0", "9999999.99")]
        public required decimal Price { get; set; }        
        [MaxLength(128)]
        public string? Img { get; set; }
        [MaxLength(64)]
        public string? CPU { get; set; }
        [Range(0, 512)]
        public int RAM { get; set; } = 0;
        [MaxLength(64)]
        public string? GPU { get; set; }
    }
}
