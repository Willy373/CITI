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
using iTextSharp.text.pdf;
using iTextSharp.text;

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
                var ingeniero = _context.DetalleInscripcions.Where(x => x.IdIngeniero == detalleInscripcion.IdIngeniero && x.Fecha.Year == detalleInscripcion.Fecha.Year);
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

        //Exportar en PDF el recibo utilizando Rotativa 

        //public async Task<IActionResult> ImprimirRecibo(int? Id)
        //{
        //    //TODO ESTO LO REEMPLAZAS CON TU PROPIA LÓGICA HACIA TU BASE DE DATOS
        //    DetalleInscripcion detalle = _context.DetalleInscripcions.Include(dv => dv.IdUsuarioNavigation).Include(dv => dv.IdIngenieroNavigation).Include(dv => dv.IdInscripcionNavigation).FirstOrDefault(dv => dv.IdDetalleInscripcion == Id);
        //    //return View();


        //    String txtQRCode = "RNI: " + detalle.IdIngenieroNavigation.Rni + ", Nombre: " +  detalle.IdIngenieroNavigation.NombreCompleto.ToString() +" pago de: "+ detalle.IdInscripcionNavigation.Tipo 
        //        +", con un monto de: "+ detalle.Total;

        //    //QRCodeGenerator qrGenerator = new QRCodeGenerator();
        //    //QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
        //    //QRCode qrCode = new QRCode(qrCodeData);
        //    //Bitmap qrcodeImage = qrCode.GetGraphic(20);

        //    //ImageConverter converter = new ImageConverter();
        //    //byte[] QrByte = (byte[])converter.ConvertTo(qrcodeImage, typeof(byte[]));
        //    //string model = Convert.ToBase64String(QrByte);


        //    QRCodeGenerator qrGenerator = new QRCodeGenerator();
        //    QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
        //    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        //    byte[] qrCodeImage = qrCode.GetGraphic(20);

        //    if(detalle.IdInscripcionNavigation.Tipo == "REAFILIACION")
        //        detalle.IdUsuarioNavigation.Password = "CINCUENTA";
        //    else
        //        detalle.IdUsuarioNavigation.Password = "DOSCIENTOS";

        //    return new ViewAsPdf("ReciboInscripcion", detalle)
        //    {
        //        FileName = $"Recibo {Id}.pdf",
        //        PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
        //        PageSize = Rotativa.AspNetCore.Options.Size.A5
        //    };
        //}

        //Funcion Para ver QR en la Vista utilizando QrCoder
        //public IActionResult VerQR(int? Id)
        //{
        //    //TODO ESTO LO REEMPLAZAS CON TU PROPIA LÓGICA HACIA TU BASE DE DATOS
        //    DetalleInscripcion detalle = _context.DetalleInscripcions.Include(dv => dv.IdUsuarioNavigation).Include(dv => dv.IdIngenieroNavigation).Include(dv => dv.IdInscripcionNavigation).FirstOrDefault(dv => dv.IdDetalleInscripcion == Id);
        //    //return View();


        //    String txtQRCode = "RNI: " + detalle.IdIngenieroNavigation.Rni + ", Nombre: " + detalle.IdIngenieroNavigation.NombreCompleto.ToString() + " pago de: " + detalle.IdInscripcionNavigation.Tipo
        //        + ", con un monto de: " + detalle.Total;

        //    QRCodeGenerator qrGenerator = new QRCodeGenerator();
        //    QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
        //    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        //    byte[] qrCodeImage = qrCode.GetGraphic(20);
        //    string model = Convert.ToBase64String(qrCodeImage);
        //    return View("VerQr", model);
        //}


        //Imprimir Recibo Con ItextSharp 
        
        
        public ActionResult ImprimirReciboItextInscripcion(int? Id)
        {
            //TODO ESTO LO REEMPLAZAS CON TU PROPIA LÓGICA HACIA TU BASE DE DATOS
            DetalleInscripcion detalle = _context.DetalleInscripcions.Include(dv => dv.IdUsuarioNavigation).Include(dv => dv.IdIngenieroNavigation).Include(dv => dv.IdInscripcionNavigation).FirstOrDefault(dv => dv.IdDetalleInscripcion == Id);
            

            String txtQRCode = "RNI: " + detalle.IdIngenieroNavigation.Rni + ", Nombre: " + detalle.IdIngenieroNavigation.NombreCompleto.ToString() + " pago de: " + detalle.IdInscripcionNavigation.Tipo
                + ", con un monto de: " + detalle.Total;
            string TotalTexto = "";
            if (detalle.IdInscripcionNavigation.Tipo == "REAFILIACION")
                TotalTexto = "CINCUENTA";
            else
                TotalTexto = "DOSCIENTOS";

            MemoryStream ms = new MemoryStream();

            Document doc = new Document(PageSize.A6, 10, 10, 10, 10);

            PdfWriter writer = PdfWriter.GetInstance(doc, ms);

            doc.Open();
            PdfPCell pdfCell = new PdfPCell();
            Paragraph para = new Paragraph();

            BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1250, BaseFont.EMBEDDED);
            iTextSharp.text.Font fontText = new iTextSharp.text.Font(bf, 7, 0, BaseColor.BLACK);
            iTextSharp.text.Font fontTextTitle = new iTextSharp.text.Font(bf, 7, 1, BaseColor.BLACK);
            iTextSharp.text.Font fontTextTitleMax = new iTextSharp.text.Font(bf, 9, 1, BaseColor.BLACK);


            PdfPTable cabecera = new PdfPTable(1);
            cabecera.TotalWidth = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
            cabecera.DefaultCell.Border = 0;

            //Agregamos la imagen del banner al documento

            iTextSharp.text.Image imageSIB = iTextSharp.text.Image.GetInstance("wwwroot/imagenes/siboruro.jpg");
            //image1.ScalePercent(50f);
            imageSIB.ScaleAbsoluteWidth(40);
            imageSIB.ScaleAbsoluteHeight(40);
            imageSIB.SetAbsolutePosition(doc.LeftMargin + 120, doc.Top - 50);
            doc.Add(imageSIB);



            pdfCell = new PdfPCell(new Paragraph("\r\n\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("\r\n\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("\nCOLEGIO DE INGENIEROS EN TECNOLOGIAS DE LA INFORMACIÓN", fontTextTitle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("Página web: citior.org.bo\r\nCorreo: finanzas@citior.org.bo\r\n\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("RECIBO OFICIAL # " + detalle.IdInscripcion + "\r\n\r\n", fontTextTitleMax));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("RNI: " + detalle.IdIngenieroNavigation.Rni, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("NOMBRE: " + detalle.IdIngenieroNavigation.NombreCompleto, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("ESPECIALIDAD: " + detalle.IdIngenieroNavigation.Especialidad, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("______________________________________________________________\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("Por Concepto de: " + detalle.IdInscripcionNavigation.Tipo, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);



            doc.Add(cabecera);



            PdfPTable table = new PdfPTable(3);

            pdfCell = new PdfPCell(new Paragraph("Nro.", fontTextTitle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(pdfCell);
            pdfCell = new PdfPCell(new Paragraph("DETALLE", fontTextTitle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(pdfCell);
            pdfCell = new PdfPCell(new Paragraph("MONTO", fontTextTitle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph(detalle.IdInscripcion.ToString(), fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph(detalle.IdInscripcionNavigation.Tipo, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph(detalle.Total.ToString(), fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(pdfCell);


            doc.Add(table);

            cabecera = new PdfPTable(4);
            pdfCell = new PdfPCell(new Paragraph());
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph());
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("Sub Total Bs:", fontTextTitle));
            pdfCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph(detalle.Total.ToString(), fontTextTitle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph());
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph());
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("Descuento Bs:", fontTextTitle));
            pdfCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("0", fontTextTitle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph());
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph());
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("Total Bs:", fontTextTitle));
            pdfCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph(detalle.Total.ToString(), fontTextTitle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            doc.Add(cabecera);


            cabecera = new PdfPTable(1);
            pdfCell = new PdfPCell(new Paragraph("______________________________________________________________\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("Son: " + TotalTexto + " CON 00/100 BOLIVIANOS", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("______________________________________________________________\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("¡Gracias por tu aporte!Estamos trabajando para brindar a todos los colegiados diferentes beneficios\r\n\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("\r\n\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("\r\n\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("\r\n\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("\r\n\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("Escanee para verificar su pago", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("Usuario: " + detalle.IdUsuarioNavigation.Cuenta, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("Fecha: " + detalle.Fecha, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            doc.Add(cabecera);


            BarcodeQRCode barcodeQRCode = new BarcodeQRCode(txtQRCode, 100, 100, null);
            iTextSharp.text.Image codeQRImage = barcodeQRCode.GetImage();
            codeQRImage.ScaleAbsolute(100, 100);
            codeQRImage.Alignment = iTextSharp.text.Image.UNDERLYING;

            //img.SetAbsolutePosition(10,100);
            codeQRImage.SetAbsolutePosition(doc.LeftMargin + 88, doc.Top - 370);

            doc.Add(codeQRImage);
            doc.Close();

            byte[] byteStream = ms.ToArray();

            ms = new MemoryStream();
            ms.Write(byteStream, 0, byteStream.Length);
            ms.Position = 0;


            return new FileStreamResult(ms, "application/pdf");
        }


    }
}
