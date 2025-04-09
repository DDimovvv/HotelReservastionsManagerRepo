using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelReservastionsManager.Data;
using HotelReservastionsManager.Models;

namespace HotelReservastionsManager.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .Include(r => r.ReservationClients)
                    .ThenInclude(rc => rc.Client);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .Include(r => r.ReservationClients)
                    .ThenInclude(rc => rc.Client)
                .FirstOrDefaultAsync(m => m.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            ViewData["RoomNumber"] = new SelectList(_context.Rooms, "RoomNumber", "RoomNumber");

            var users = _context.Set<User>()
                .Where(u => u.Active)
                .Select(u => new
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName} ({u.UserName})"
                });

            ViewData["UserId"] = new SelectList(users, "Id", "FullName");

            return View();
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ReservationId,RoomNumber,UserId,CheckInDate,CheckOutDate,BreakfastIncluded,AllInclusive,FinalPrice")] Reservation reservation,
            List<string> ClientFirstName,
            List<string> ClientLastName,
            List<string> ClientPhone,
            List<string> ClientEmail,
            List<bool> ClientAdult)
        {
            if (reservation.CheckInDate >= reservation.CheckOutDate)
            {
                ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date");
            }
            if (Request.Form.TryGetValue("FinalPrice", out var fpVal))
            {
                double.TryParse(fpVal.ToString().Replace('.', ','), out var finalPrice);
                reservation.FinalPrice = finalPrice;
                ModelState.Remove("FinalPrice");
            }
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    _context.Add(reservation);
                    await _context.SaveChangesAsync();

                    for (int i = 0; i < ClientFirstName.Count; i++)
                    {
                        if (string.IsNullOrEmpty(ClientFirstName[i]) ||
                            string.IsNullOrEmpty(ClientLastName[i]) ||
                            string.IsNullOrEmpty(ClientPhone[i]) ||
                            string.IsNullOrEmpty(ClientEmail[i]))
                        {
                            continue;
                        }

                        var existingClient = await _context.Set<Client>()
                            .FirstOrDefaultAsync(c => c.PhoneNumber == ClientPhone[i]);

                        Client client;

                        if (existingClient != null)
                        {
                            client = existingClient;
                            client.FirstName = ClientFirstName[i];
                            client.LastName = ClientLastName[i];
                            client.Email = ClientEmail[i];
                            client.Adult = i < ClientAdult.Count ? ClientAdult[i] : true;

                            _context.Update(client);
                        }
                        else
                        {
                            client = new Client
                            {
                                FirstName = ClientFirstName[i],
                                LastName = ClientLastName[i],
                                PhoneNumber = ClientPhone[i],
                                Email = ClientEmail[i],
                                Adult = i < ClientAdult.Count ? ClientAdult[i] : true
                            };

                            _context.Add(client);
                            await _context.SaveChangesAsync();
                        }

                        var reservationClient = new ReservationClients
                        {
                            ReservationId = reservation.ReservationId,
                            ClientId = client.ClientId
                        };

                        _context.Add(reservationClient);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            ViewData["RoomNumber"] = new SelectList(_context.Rooms, "RoomNumber", "RoomNumber", reservation.RoomNumber);

            var users = _context.Set<User>()
                .Where(u => u.Active)
                .Select(u => new
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName} ({u.UserName})"
                });

            ViewData["UserId"] = new SelectList(users, "Id", "FullName", reservation.UserId);

            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.ReservationClients)
                    .ThenInclude(rc => rc.Client)
                .FirstOrDefaultAsync(m => m.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            ViewData["RoomNumber"] = new SelectList(_context.Rooms, "RoomNumber", "RoomNumber", reservation.RoomNumber);

            var users = _context.Set<User>()
                .Where(u => u.Active)
                .Select(u => new
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName} ({u.UserName})"
                });

            ViewData["UserId"] = new SelectList(users, "Id", "FullName", reservation.UserId);

            ViewData["Clients"] = reservation.ReservationClients
                .Select(rc => rc.Client)
                .ToList();

            return View(reservation);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ReservationId,RoomNumber,UserId,CheckInDate,CheckOutDate,BreakfastIncluded,AllInclusive,FinalPrice")] Reservation reservation,
            List<string> ClientFirstName,
            List<string> ClientLastName,
            List<string> ClientPhone,
            List<string> ClientEmail,
            List<bool> ClientAdult,
            List<int> ClientId)
        {
            if (id != reservation.ReservationId)
            {
                return NotFound();
            }

            if (reservation.CheckInDate >= reservation.CheckOutDate)
            {
                ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    _context.Update(reservation);

                    var existingRelationships = await _context.Set<ReservationClients>()
                        .Where(rc => rc.ReservationId == reservation.ReservationId)
                        .ToListAsync();

                    _context.RemoveRange(existingRelationships);
                    await _context.SaveChangesAsync();

                    for (int i = 0; i < ClientFirstName.Count; i++)
                    {
                        if (string.IsNullOrEmpty(ClientFirstName[i]) ||
                            string.IsNullOrEmpty(ClientLastName[i]) ||
                            string.IsNullOrEmpty(ClientPhone[i]) ||
                            string.IsNullOrEmpty(ClientEmail[i]))
                        {
                            continue;
                        }

                        Client client;
                        int clientId = i < ClientId.Count ? ClientId[i] : 0;

                        if (clientId > 0)
                        {
                            client = await _context.Set<Client>().FindAsync(clientId);

                            if (client != null)
                            {
                                client.FirstName = ClientFirstName[i];
                                client.LastName = ClientLastName[i];
                                client.PhoneNumber = ClientPhone[i];
                                client.Email = ClientEmail[i];
                                client.Adult = i < ClientAdult.Count ? ClientAdult[i] : true;

                                _context.Update(client);
                            }
                            else
                            {
                                client = new Client
                                {
                                    FirstName = ClientFirstName[i],
                                    LastName = ClientLastName[i],
                                    PhoneNumber = ClientPhone[i],
                                    Email = ClientEmail[i],
                                    Adult = i < ClientAdult.Count ? ClientAdult[i] : true
                                };

                                _context.Add(client);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            client = await _context.Set<Client>()
                                .FirstOrDefaultAsync(c => c.PhoneNumber == ClientPhone[i]);

                            if (client != null)
                            {
                                client.FirstName = ClientFirstName[i];
                                client.LastName = ClientLastName[i];
                                client.Email = ClientEmail[i];
                                client.Adult = i < ClientAdult.Count ? ClientAdult[i] : true;

                                _context.Update(client);
                            }
                            else
                            {
                                client = new Client
                                {
                                    FirstName = ClientFirstName[i],
                                    LastName = ClientLastName[i],
                                    PhoneNumber = ClientPhone[i],
                                    Email = ClientEmail[i],
                                    Adult = i < ClientAdult.Count ? ClientAdult[i] : true
                                };

                                _context.Add(client);
                                await _context.SaveChangesAsync();
                            }
                        }

                        var reservationClient = new ReservationClients
                        {
                            ReservationId = reservation.ReservationId,
                            ClientId = client.ClientId
                        };

                        _context.Add(reservationClient);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.ReservationId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            ViewData["RoomNumber"] = new SelectList(_context.Rooms, "RoomNumber", "RoomNumber", reservation.RoomNumber);

            var users = _context.Set<User>()
                .Where(u => u.Active)
                .Select(u => new
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName} ({u.UserName})"
                });

            ViewData["UserId"] = new SelectList(users, "Id", "FullName", reservation.UserId);

            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .Include(r => r.ReservationClients)
                    .ThenInclude(rc => rc.Client)
                .FirstOrDefaultAsync(m => m.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var relationships = await _context.Set<ReservationClients>()
                    .Where(rc => rc.ReservationId == id)
                    .ToListAsync();

                _context.RemoveRange(relationships);

                var reservation = await _context.Reservations.FindAsync(id);
                if (reservation != null)
                {
                    _context.Reservations.Remove(reservation);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ReservationId == id);
        }
        [HttpGet]
        public JsonResult GetRoomPrices()
        {
            var rooms = _context.Rooms.Select(r => new
            {
                r.RoomNumber,
                r.AdultPrice,
                r.ChildPrice
            }).ToDictionary(r => r.RoomNumber.ToString(), r => new
            {
                r.AdultPrice,
                r.ChildPrice
            });

            return Json(rooms);
        }
    }
}