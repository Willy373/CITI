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
    public class CuotumsController : Controller
    {
        private readonly CitiContext _context;

        public CuotumsController(CitiContext context)
        {
            _context = context;
        }

        // GET: Cuotums
        public async Task<IActionResult> Index()
        {
            var citiContext = _context.Cuota.Include(c => c.IdIngenieroNavigation).Include(c => c.IdUsuarioNavigation);
            return View(await citiContext.ToListAsync());
        }

        // GET: Cuotums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Cuota == null)
            {
                return NotFound();
            }

            var cuotum = await _context.Cuota
                .Include(c => c.IdIngenieroNavigation)
                .Include(c => c.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdCuota == id);
            if (cuotum == null)
            {
                return NotFound();
            }

            return View(cuotum);
        }

        // GET: Cuotums/Create
        public IActionResult Create()
        {
            ViewData["IdIngeniero"] = new SelectList(_context.Ingenieros, "IdIngeniero", "NombreCompleto");
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario");
            return View();
        }

        // POST: Cuotums/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCuota,Detalle,Fecha,MesesTotal,Total,IdUsuario,IdIngeniero")] Cuotum cuotum)
        {
            if (ModelState.IsValid)
            {
                //Para hacer los detalles de la cuota
                cuotum.Total = cuotum.MesesTotal * 10;
                cuotum.IdUsuarioNavigation = _context.Usuarios.Find(1);
                List<String> mesActual = MesMostrar(cuotum.MesesTotal);
                cuotum.Detalle = "";
                foreach (var item in mesActual)
                {
                   cuotum.Detalle = cuotum.Detalle + item.ToString() + " ";
                }
                _context.Add(cuotum);
                await _context.SaveChangesAsync();


                //// Para hacer las cuotas
                Cuotum cuota = new Cuotum();
                cuota = _context.Cuota.OrderByDescending(x => x.IdCuota).First();
                
                foreach (var item in mesActual)
                {
                    DetalleCuotum detalleCuotum = new DetalleCuotum();
                    detalleCuotum.IdCuota = cuota.IdCuota;
                    detalleCuotum.DetalleMes = item.ToString();
                    detalleCuotum.Monto = 10;
                    _context.Add(detalleCuotum);
                    await _context.SaveChangesAsync();
                }
                

                return RedirectToAction(nameof(Index));
            }
            ViewData["IdIngeniero"] = new SelectList(_context.Ingenieros, "IdIngeniero", "NombreCompleto", cuotum.IdIngeniero);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", cuotum.IdUsuario);
            return View(cuotum);
        }

        // GET: Cuotums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Cuota == null)
            {
                return NotFound();
            }

            var cuotum = await _context.Cuota.FindAsync(id);
            if (cuotum == null)
            {
                return NotFound();
            }
            ViewData["IdIngeniero"] = new SelectList(_context.Ingenieros, "IdIngeniero", "NombreCompleto", cuotum.IdIngeniero);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", cuotum.IdUsuario);
            return View(cuotum);
        }

        // POST: Cuotums/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCuota,Detalle,Fecha,MesesTotal,Total,IdUsuario,IdIngeniero")] Cuotum cuotum)
        {
            if (id != cuotum.IdCuota)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuotum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuotumExists(cuotum.IdCuota))
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
            ViewData["IdIngeniero"] = new SelectList(_context.Ingenieros, "IdIngeniero", "NombreCompleto", cuotum.IdIngeniero);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", cuotum.IdUsuario);
            return View(cuotum);
        }

        // GET: Cuotums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Cuota == null)
            {
                return NotFound();
            }

            var cuotum = await _context.Cuota
                .Include(c => c.IdIngenieroNavigation)
                .Include(c => c.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdCuota == id);
            if (cuotum == null)
            {
                return NotFound();
            }

            return View(cuotum);
        }

        // POST: Cuotums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Cuota == null)
            {
                return Problem("Entity set 'CitiContext.Cuota'  is null.");
            }
            var cuotum = await _context.Cuota.FindAsync(id);
            if (cuotum != null)
            {
                _context.Cuota.Remove(cuotum);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuotumExists(int id)
        {
          return (_context.Cuota?.Any(e => e.IdCuota == id)).GetValueOrDefault();
        }

        
        List<string> MesMostrar(int numero)
        {
            List<String> mesActual = new List<string>();
            String[] mes = { "ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO", "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE" };
            int numMes = 0;
            int anio = 2022;
            mesActual.Add(mes[9] + " - " + anio);
            mesActual.Add(mes[10] + " - " + anio);
            mesActual.Add(mes[11] + " - " + anio);
            numero = numero - 3;
            anio = anio + 1;
            for (int i = 0; i < numero; i++)
            {
                if (numMes != 12)
                {
                    mesActual.Add(mes[numMes] + " - " + anio);
                    numMes = numMes + 1;
                }
                else
                {
                    numMes = 0;
                    anio = anio + 1;
                }
            }
            return mesActual;
        }
    }
}
