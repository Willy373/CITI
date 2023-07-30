using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CITI_Oruro.Models;

namespace CITI_Oruro.Controllers
{
    public class IngenieroesController : Controller
    {
        private readonly CitiContext _context;

        public IngenieroesController(CitiContext context)
        {
            _context = context;
        }

        // GET: Ingenieroes
        public async Task<IActionResult> Index()
        {
              return _context.Ingenieros != null ? 
                          View(await _context.Ingenieros.ToListAsync()) :
                          Problem("Entity set 'CitiContext.Ingenieros'  is null.");
        }

        // GET: Ingenieroes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Ingenieros == null)
            {
                return NotFound();
            }

            var ingeniero = await _context.Ingenieros
                .FirstOrDefaultAsync(m => m.IdIngeniero == id);
            if (ingeniero == null)
            {
                return NotFound();
            }

            return View(ingeniero);
        }

        // GET: Ingenieroes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ingenieroes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdIngeniero,NombreCompleto,Especialidad,Rni,Ci,Fecha")] Ingeniero ingeniero)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ingeniero);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ingeniero);
        }

        // GET: Ingenieroes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Ingenieros == null)
            {
                return NotFound();
            }

            var ingeniero = await _context.Ingenieros.FindAsync(id);
            if (ingeniero == null)
            {
                return NotFound();
            }
            return View(ingeniero);
        }

        // POST: Ingenieroes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdIngeniero,NombreCompleto,Especialidad,Rni,Ci,Fecha")] Ingeniero ingeniero)
        {
            if (id != ingeniero.IdIngeniero)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ingeniero);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IngenieroExists(ingeniero.IdIngeniero))
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
            return View(ingeniero);
        }

        // GET: Ingenieroes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Ingenieros == null)
            {
                return NotFound();
            }

            var ingeniero = await _context.Ingenieros
                .FirstOrDefaultAsync(m => m.IdIngeniero == id);
            if (ingeniero == null)
            {
                return NotFound();
            }

            return View(ingeniero);
        }

        // POST: Ingenieroes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Ingenieros == null)
            {
                return Problem("Entity set 'CitiContext.Ingenieros'  is null.");
            }
            var ingeniero = await _context.Ingenieros.FindAsync(id);
            if (ingeniero != null)
            {
                _context.Ingenieros.Remove(ingeniero);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IngenieroExists(int id)
        {
          return (_context.Ingenieros?.Any(e => e.IdIngeniero == id)).GetValueOrDefault();
        }
    }
}
