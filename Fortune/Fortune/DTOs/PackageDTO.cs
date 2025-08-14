using System.ComponentModel.DataAnnotations;

namespace Fortune.DTOs
{
    public class PackageDTO
    {
        [Required]
        public string description { get; set; }
        [Required]
        public int price { get; set; }
    }
}
