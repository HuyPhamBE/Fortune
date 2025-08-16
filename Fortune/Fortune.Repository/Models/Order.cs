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
    public enum OrderStatus { Pending, Paid, Canceled, Expired }
    [Table("Order")]
    public class Order
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Column("ordercode")]
        public long OrderCode { get; set; }
        [Column("amount")]
        public int Amount { get; set; }
        [Column("packageid")]
        public Guid PackageId { get; set; }
        [Column("description")]
        public string Description { get; set; }

        [Column("userid")]
        public Guid? UserId { get; set; }
        [Column("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        [Column("GuestEmail")]
        public string? GuestEmail { get; set; }

        [Column("checkouturl")]
        public string CheckoutUrl { get; set; }
        [Column("paymentlinkid")]
        public string PaymentLinkId { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("paidat")]
        public DateTime? PaidAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool contact { get; set; } =false;
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;
        [ForeignKey("PackageId")]
        [JsonIgnore]
        public Package Package { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; }
    }
}
