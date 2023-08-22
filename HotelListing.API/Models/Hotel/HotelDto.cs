using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Models.Hotel
{
    public class HotelDto : BaseHotelDto
    {
        [Required]
        public int Id { get; set; }
    }
}
