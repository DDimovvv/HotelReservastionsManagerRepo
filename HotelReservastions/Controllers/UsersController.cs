using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservastionsManager.Models;
using HotelReservastionsManager.Data;
using Microsoft.AspNetCore.Identity;

namespace HotelReservastionsManager.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index(string searchEgn)
        {
            var users = from u in _context.Users
                        select u;

            if (!string.IsNullOrEmpty(searchEgn))
            {
                users = users.Where(u => u.EGN.Contains(searchEgn));
                ViewData["CurrentFilter"] = searchEgn;
            }

            return View(await users.ToListAsync());
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            var user = new User
            {
                Active = true,
                HireDate = DateOnly.FromDateTime(DateTime.Today)
            };
            return View(user);
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EGN,FirstName,MiddleName,LastName,HireDate,Active,Email,PhoneNumber,UserName")] User user, string password)
        {
            if (ModelState.IsValid)
            {
                user.Id = Guid.NewGuid().ToString();
                user.Active = true;

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,EGN,FirstName,MiddleName,LastName,HireDate,Active,ReleaseDate,Email,PhoneNumber,UserName")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.EGN = user.EGN;
                    existingUser.FirstName = user.FirstName;
                    existingUser.MiddleName = user.MiddleName;
                    existingUser.LastName = user.LastName;
                    existingUser.HireDate = user.HireDate;
                    existingUser.Active = user.Active;
                    existingUser.ReleaseDate = user.ReleaseDate;
                    existingUser.Email = user.Email;
                    existingUser.NormalizedEmail = user.Email?.ToUpper();
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.UserName = user.UserName;
                    existingUser.NormalizedUserName = user.UserName?.ToUpper();

                    if (existingUser.Active && !user.Active)
                    {
                        existingUser.Active = false;
                        existingUser.ReleaseDate = user.ReleaseDate ?? DateOnly.FromDateTime(DateTime.Today);
                    }
                    else if (!existingUser.Active && user.Active)
                    {
                        existingUser.Active = true;
                        existingUser.ReleaseDate = null;
                    }
                    else
                    {
                        existingUser.Active = user.Active;
                        existingUser.ReleaseDate = user.ReleaseDate;
                    }

                    if (existingUser.Active)
                    {
                        existingUser.ReleaseDate = null;
                    }
                    var result = await _userManager.UpdateAsync(existingUser);

                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: User/SearchByEGN
        public async Task<IActionResult> SearchByEGN(string egn)
        {
            if (string.IsNullOrEmpty(egn))
            {
                return View("Index", await _context.Users.ToListAsync());
            }

            var users = await _context.Users
                .Where(u => u.EGN == egn)
                .ToListAsync();

            return View("Index", users);
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}