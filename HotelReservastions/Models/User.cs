using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HotelReservastionsManager.Models
{
    public class User:IdentityUser
    {
        [Required]
        [StringLength(10), MinLength(10)]
        public string EGN { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string MiddleName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 10)]
        public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }
        [Required]
        public DateOnly HireDate { get; set; }
        public bool Active { get; set; }
        public DateOnly? ReleaseDate { get; set; } = null;
    }
}
