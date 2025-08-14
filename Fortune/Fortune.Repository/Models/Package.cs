using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fortune.Repository.Models
{
    [Table("package")]
    public class Package
    {
        [Key]
        [Column("package_id")]
        public Guid package_Id { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public int price { get; set; }
        [InverseProperty("Package")]
        [JsonIgnore]
        public ICollection<Order> Orders { get; set; }
    }
}
