using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECinema.Web.Data;
using ECinema.Web.Models.DomainModels;
using ECinema.Web.Models.DomainModels.Enumerations;
using ECinema.Web.Models.DTO;

namespace ECinema.Web.Controllers 
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tickets
        //public async Task<IActionResult> Index()
        //{
        //    var tickets = await _context.Tickets.Include(t => t.movie).ToListAsync();
        //    return View(tickets);
        //}
        public async Task<IActionResult> Index(DateTime? filterDate)
        {
            var ticketsQuery = _context.Tickets.AsQueryable();

            if (filterDate.HasValue)
            {
                var filterDateValue = filterDate.Value.Date;
                ticketsQuery = ticketsQuery.Where(t => t.MovieDate.Date == filterDateValue);
            }

            var tickets = await ticketsQuery.Include(t => t.movie).ToListAsync();

            // For debugging purposes, check the number of tickets and the filterDate value
            var count = tickets.Count;
            var selectedDate = filterDate?.ToString("yyyy-MM-dd") ?? "None";
            Console.WriteLine($"Tickets Count: {count}, Selected Date: {selectedDate}");

            return View(tickets);
        }




        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.movie) // Include the associated movie
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            Console.WriteLine("MovieName: " + ticket.movie.MovieName);

            return View(ticket);
        }


        // GET: Tickets/Create
        public IActionResult Create()
        {
            CreateTicketDto dto = new CreateTicketDto
            {
                AllActors = _context.Actors,
                AllMovies = _context.Movies,
                ticket = new Ticket()
            };
            return View(dto);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketDto dto)
        {
            if (ModelState.IsValid)
            {
                Ticket ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    MovieDate = dto.ticket.MovieDate,
                    movie = _context.Movies.FirstOrDefault(m => m.Id == dto.movieId),
                    TicketPrice = (float)dto.TicketPrice // Set the TicketPrice property
                };

                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If the model state is not valid, return the view with the same model
            // so that the user can correct the input.
            dto.AllMovies = _context.Movies;
            return View(dto);
        }



        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,MovieDate,TicketPrice")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
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
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(Guid id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}
