using System.ComponentModel.DataAnnotations;

namespace Fortune.DTOs
{
    public class BookingDTO
    {
        public string description { get; set; }
        public bool type { get; set; }

        public long status { get; set; }

        public Guid staff_id { get; set; }

        public Guid? minigame_id { get; set; }
        public Guid? plan_id { get; set; }
    }
}
