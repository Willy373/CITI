using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CITI_Oruro.Models;
using Microsoft.Win32;
using Rotativa.AspNetCore;
using QRCoder;
using System.Drawing;
using Microsoft.IdentityModel.Tokens;
using static System.Net.Mime.MediaTypeNames;

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
                var ingeniero = _context.DetalleInscripcions.Where(x => x.IdIngeniero == detalleInscripcion.IdIngeniero && x.Fecha.Year == DateTime.Now.Year);
                if (ingeniero.IsNullOrEmpty()) {
                    if (detalleInscripcion.IdInscripcion == 1)
                        detalleInscripcion.Total = 50;
                    else
                        detalleInscripcion.Total = 200;
                    detalleInscripcion.IdUsuarioNavigation = _context.Usuarios.Find(1);

                    _context.Add(detalleInscripcion);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["ValidateMessage"] = "Usuario Ya Registrado";
                }
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


        public async Task<IActionResult> ImprimirRecibo(int? Id)
        {
            //TODO ESTO LO REEMPLAZAS CON TU PROPIA LÓGICA HACIA TU BASE DE DATOS
            DetalleInscripcion detalle = _context.DetalleInscripcions.Include(dv => dv.IdUsuarioNavigation).Include(dv => dv.IdIngenieroNavigation).Include(dv => dv.IdInscripcionNavigation).FirstOrDefault(dv => dv.IdDetalleInscripcion == Id);
            //return View();


            String txtQRCode = "RNI: " + detalle.IdIngenieroNavigation.Rni + ", Nombre: " +  detalle.IdIngenieroNavigation.NombreCompleto.ToString() +" pago de: "+ detalle.IdInscripcionNavigation.Tipo 
                +", con un monto de: "+ detalle.Total;

            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
            //QRCode qrCode = new QRCode(qrCodeData);
            //Bitmap qrcodeImage = qrCode.GetGraphic(20);

            //ImageConverter converter = new ImageConverter();
            //byte[] QrByte = (byte[])converter.ConvertTo(qrcodeImage, typeof(byte[]));
            //string model = Convert.ToBase64String(QrByte);


            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);

            if(detalle.IdInscripcionNavigation.Tipo == "REAFILIACION")
                detalle.IdUsuarioNavigation.Password = "CINCUENTA";
            else
                detalle.IdUsuarioNavigation.Password = "DOSCIENTOS";

            return new ViewAsPdf("ReciboInscripcion", detalle)
            {
                FileName = $"Recibo {Id}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A5
            };
        }

        public IActionResult VerQR(int? Id)
        {
            //TODO ESTO LO REEMPLAZAS CON TU PROPIA LÓGICA HACIA TU BASE DE DATOS
            DetalleInscripcion detalle = _context.DetalleInscripcions.Include(dv => dv.IdUsuarioNavigation).Include(dv => dv.IdIngenieroNavigation).Include(dv => dv.IdInscripcionNavigation).FirstOrDefault(dv => dv.IdDetalleInscripcion == Id);
            //return View();


            String txtQRCode = "RNI: " + detalle.IdIngenieroNavigation.Rni + ", Nombre: " + detalle.IdIngenieroNavigation.NombreCompleto.ToString() + " pago de: " + detalle.IdInscripcionNavigation.Tipo
                + ", con un monto de: " + detalle.Total;

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            string model = Convert.ToBase64String(qrCodeImage);
            return View("VerQr", model);
        }

        
    }
}
