using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelReservastionsManager.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }
        [ForeignKey("Room")]
        public int RoomNumber { get; set; }
        public Room? Room { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User? User { get; set; }
        [Required]
        public DateTime CheckInDate { get; set; }
        [Required]
        public DateTime CheckOutDate { get; set; }
        public bool BreakfastIncluded { get; set; }
        public bool AllInclusive { get; set; }
        [Required]
        public double FinalPrice { get; set; }
        public ICollection<ReservationClients>? ReservationClients { get; set; }
    }
}
