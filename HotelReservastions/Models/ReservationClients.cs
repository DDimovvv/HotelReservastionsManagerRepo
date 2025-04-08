namespace HotelReservastionsManager.Models
{
    public class ReservationClients
    {
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
        public int ClientId { get; set; }
        public Client? Client { get; set; }
    }
}
