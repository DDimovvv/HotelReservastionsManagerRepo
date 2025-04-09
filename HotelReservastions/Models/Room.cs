using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Adult price must be greater than 0")]
        public decimal AdultPrice { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Child price must be greater than 0")]
        public decimal ChildPrice { get; set; }
        public ICollection<Reservation>? Reservations { get; set; }
    }
}
