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
    public class DetalleInscripcionsController : Controller
    {
        private readonly CitiContext _context;

        public DetalleInscripcionsController(CitiContext context)
        {
            _context = context;
        }

        // GET: DetalleInscripcions
        //public async Task<IActionResult> Index()
        //{
        //    var citiContext = _context.DetalleInscripcions.Include(d => d.IdIngenieroNavigation).Include(d => d.IdInscripcionNavigation).Include(d => d.IdUsuarioNavigation);
        //    return View(await citiContext.ToListAsync());
        //}

        public async Task<IActionResult> Index(string buscar)
        {
            var detalle = from ingeniero in _context.DetalleInscripcions.Include(d => d.IdIngenieroNavigation).Include(d => d.IdInscripcionNavigation).Include(d => d.IdUsuarioNavigation)
                          select ingeniero;
            if (!String.IsNullOrEmpty(buscar))
            {
                detalle = detalle.Where(x => x.IdIngenieroNavigation.NombreCompleto.Contains(buscar));
            }

            return View(await detalle.ToListAsync());
        }

        // GET: DetalleInscripcions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DetalleInscripcions == null)
            {
                return NotFound();
            }

            var detalleInscripcion = await _context.DetalleInscripcions
                .Include(d => d.IdIngenieroNavigation)
                .Include(d => d.IdInscripcionNavigation)
                .Include(d => d.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdDetalleInscripcion == id);
            if (detalleInscripcion == null)
            {
                return NotFound();
            }

            return View(detalleInscripcion);
        }

        // GET: DetalleInscripcions/Create
        public IActionResult Create()
        {
            ViewData["IdIngeniero"] = new SelectList(_context.Ingenieros, "IdIngeniero", "NombreCompleto");
            ViewData["IdInscripcion"] = new SelectList(_context.Inscripcions, "IdInscripcion", "Tipo");
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario");
            return View();
        }

        // POST: DetalleInscripcions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDetalleInscripcion,Fecha,Total,IdUsuario,IdIngeniero,IdInscripcion")] DetalleInscripcion detalleInscripcion)
        {
            if (ModelState.IsValid)
            {
                
                if (detalleInscripcion.IdInscripcion == 1)
                    detalleInscripcion.Total = 50;
                else
                    detalleInscripcion.Total = 200;
                detalleInscripcion.IdUsuarioNavigation = _context.Usuarios.Find(1);

                _context.Add(detalleInscripcion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdIngeniero"] = new SelectList(_context.Ingenieros, "IdIngeniero", "NombreCompleto", detalleInscripcion.IdIngeniero);
            ViewData["IdInscripcion"] = new SelectList(_context.Inscripcions, "IdInscripcion", "Tipo", detalleInscripcion.IdInscripcion);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", detalleInscripcion.IdUsuario);
            return View(detalleInscripcion);
        }

        // GET: DetalleInscripcions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DetalleInscripcions == null)
            {
                return NotFound();
            }

            var detalleInscripcion = await _context.DetalleInscripcions.FindAsync(id);
            if (detalleInscripcion == null)
            {
                return NotFound();
            }
            ViewData["IdIngeniero"] = new SelectList(_context.Ingenieros, "IdIngeniero", "NombreCompleto", detalleInscripcion.IdIngeniero);
            ViewData["IdInscripcion"] = new SelectList(_context.Inscripcions, "IdInscripcion", "Tipo", detalleInscripcion.IdInscripcion);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", detalleInscripcion.IdUsuario);
            return View(detalleInscripcion);
        }

        // POST: DetalleInscripcions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDetalleInscripcion,Fecha,Total,IdUsuario,IdIngeniero,IdInscripcion")] DetalleInscripcion detalleInscripcion)
        {
            if (id != detalleInscripcion.IdDetalleInscripcion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (detalleInscripcion.IdInscripcion == 1)
                        detalleInscripcion.Total = 50;
                    else
                        detalleInscripcion.Total = 200;
                    detalleInscripcion.IdUsuarioNavigation = _context.Usuarios.Find(1);

                    _context.Update(detalleInscripcion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetalleInscripcionExists(detalleInscripcion.IdDetalleInscripcion))
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
            ViewData["IdIngeniero"] = new SelectList(_context.Ingenieros, "IdIngeniero", "NombreCompleto", detalleInscripcion.IdIngeniero);
            ViewData["IdInscripcion"] = new SelectList(_context.Inscripcions, "IdInscripcion", "Tipo", detalleInscripcion.IdInscripcion);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", detalleInscripcion.IdUsuario);
            return View(detalleInscripcion);
        }

        // GET: DetalleInscripcions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DetalleInscripcions == null)
            {
                return NotFound();
            }

            var detalleInscripcion = await _context.DetalleInscripcions
                .Include(d => d.IdIngenieroNavigation)
                .Include(d => d.IdInscripcionNavigation)
                .Include(d => d.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdDetalleInscripcion == id);
            if (detalleInscripcion == null)
            {
                return NotFound();
            }

            return View(detalleInscripcion);
        }

        // POST: DetalleInscripcions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DetalleInscripcions == null)
            {
                return Problem("Entity set 'CitiContext.DetalleInscripcions'  is null.");
            }
            var detalleInscripcion = await _context.DetalleInscripcions.FindAsync(id);
            if (detalleInscripcion != null)
            {
                _context.DetalleInscripcions.Remove(detalleInscripcion);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetalleInscripcionExists(int id)
        {
          return (_context.DetalleInscripcions?.Any(e => e.IdDetalleInscripcion == id)).GetValueOrDefault();
        }
    }
}
