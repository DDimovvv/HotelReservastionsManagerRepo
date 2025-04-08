using System.ComponentModel.DataAnnotations;

namespace HotelReservastionsManager.Models
{
    public class Room
    {
        [Key]
        public int RoomNumber { get; set; }
        [Range(2, 10)]
        [Required]
        public int Capacity { get; set; }
        public enum RoomTypes { Two_Single_Beds, Apartment, Double_Bed, Penthouse, Maissonette }
        public RoomTypes RoomType { get; set; }
        public bool IsAvailable { get; set; }
        [Required]
        public double AdultPrice { get; set; }
        [Required]
        public double ChildPrice { get; set; }
        public ICollection<Reservation>? Reservations { get; set; }
    }
}
