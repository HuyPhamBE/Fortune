using Fortune.Repository.Models;

namespace Fortune.Controllers
{ 
    public class OrderDTO
    {
        public int? Amount { get; set; }
        public string? Description { get; set; }
        public OrderStatus Status { get; set; }
    }
}
