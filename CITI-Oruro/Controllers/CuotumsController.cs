using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CITI_Oruro.Models;
using QRCoder;
using Rotativa.AspNetCore;
using CITI_Oruro.Models.ViewModel;
using iTextSharp.text.pdf;
using iTextSharp.text;

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
            int mes2022 = 9;
            while(numMes < numero)
            {
                if (numMes <= 2)
                {
                    mesActual.Add(mes[mes2022] + " - " + anio);
                    mes2022++;
                    numMes++;
                }
                else
                {
                    numMes++;
                }
            }
            numMes = 0;
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


        public string Literal(int num)
        {
            string[] lit_unidades = { "", "cero", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve", "diez" };
            string[] lit_diez = { "once", "doce", "trece", "catorce", "quince", "dieciseis", "diecisiete", "dieciocho", "diecinueve" };

            string[] lit_decenas = { "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa", "cien" };
            string[] lit_centenas = { "ciento", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };

            string res = "";
            if (num <= 10) return lit_unidades[num + 1];
            if (num < 20) return lit_diez[num - 11];
            if (num < 30) return "veinti" + lit_unidades[(num % 10) + 1];
            if (num <= 100)
            {
                res = lit_decenas[(num / 10) - 2];
                if (num % 10 > 0) res += " y " + lit_unidades[(num % 10) + 1];
                return res;
            }
            if (num < 1000) return lit_centenas[(num / 100) - 1] + " " + Literal(num % 100);
            if (num < 2000) return " mil " + Literal(num % 1000);
            if (num < 10000) return lit_unidades[(num / 1000) + 1] + " mil " + Literal(num % 1000);
            else
                return "";
        }


        //Exportar en PDF el recibo utilizando Rotativa 

        //public async Task<IActionResult> ImprimirCuota(int? Id)
        //{

        //    //TODO ESTO LO REEMPLAZAS CON TU PROPIA LÓGICA HACIA TU BASE DE DATOS
        //    ViewModelCuotum cuota = _context.Cuota.Include(dv => dv.DetalleCuota).Where(dv => dv.IdCuota == Id)
        //        .Select(v => new ViewModelCuotum()
        //        {
        //            IdCuota = v.IdCuota,
        //            Fecha = v.Fecha,
        //            MesesTotal = v.MesesTotal,
        //            Total = v.Total,
        //            IdUsuario = v.IdUsuario,
        //            IdIngeniero = v.IdIngeniero,
        //            IdIngenieroNavigation = v.IdIngenieroNavigation,
        //            IdUsuarioNavigation = v.IdUsuarioNavigation,
        //            DetalleCuota = v.DetalleCuota.Select(dv => new ViewModelDetalleCuotum()
        //            {
        //                //IdDetalleCuota = dv.IdDetalleCuota,
        //                DetalleMes = dv.DetalleMes,
        //                Monto = dv.Monto,
        //            }).ToList(),
        //        }).FirstOrDefault();
        //        int num = Convert.ToInt32(cuota.Total);
        //        String numLetras = Literal(num);
        //        cuota.IdUsuarioNavigation.Password = numLetras.ToUpper();

        //    return new ViewAsPdf("ReciboCuota", cuota)
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
        //    Cuotum cuotum = _context.Cuota.Include(dv => dv.IdIngenieroNavigation).Include(dv => dv.IdUsuarioNavigation).FirstOrDefault(dv => dv.IdCuota == Id);
        //    //return View();


        //    String txtQRCode = "RNI: " + cuotum.IdIngenieroNavigation.Rni + ", Nombre: " + cuotum.IdIngenieroNavigation.NombreCompleto.ToString() + " pago de: Cuota de " + cuotum.MesesTotal.ToString()
        //        + " meses, con un monto de: " + cuotum.Total;

        //    QRCodeGenerator qrGenerator = new QRCodeGenerator();
        //    QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
        //    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        //    byte[] qrCodeImage = qrCode.GetGraphic(20);
        //    string model = Convert.ToBase64String(qrCodeImage);
        //    return View("VerQr", model);
        //}




        public List<Ingeniero> IngenierosAlDia()
        {
            var ingenieros = _context.Ingenieros.ToList();
            List<Ingeniero> ingDia = new List<Ingeniero>();
            foreach (var item in ingenieros)
            {
                var recibo = _context.Cuota.Include(dv => dv.IdIngenieroNavigation).Where(dv => dv.IdIngeniero == item.IdIngeniero);
                int cuota = 0;
                foreach (var itemCuota in recibo)
                {
                    cuota = cuota + itemCuota.MesesTotal;
                }
                int mesActual = DateTime.Now.Month + 3;
                if (cuota >= mesActual)
                    ingDia.Add(item);
            }
            return ingDia;
        }
        public List<Ingeniero> IngenierosDeudores()
        {
            var ingenieros = _context.Ingenieros.ToList();
            List<Ingeniero> ingDeudor = new List<Ingeniero>();
            foreach (var item in ingenieros)
            {
                var recibo = _context.Cuota.Include(dv => dv.IdIngenieroNavigation).Where(dv => dv.IdIngeniero == item.IdIngeniero);
                int cuota = 0;
                foreach (var itemCuota in recibo)
                {
                    cuota = cuota + itemCuota.MesesTotal;
                }
                if (cuota < 6)
                    ingDeudor.Add(item);
            }
            return ingDeudor;
        }

        public IActionResult Reportes()
        {
            return View();
        }

        public IActionResult ReporteDeudores()
        {

            List<Ingeniero> Ingeniero = new List<Ingeniero>();
            Ingeniero = IngenierosDeudores();

            return new ViewAsPdf("ReporteDeudores", Ingeniero)
            {
                FileName = $"Deudores {1}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.Letter
            };
        }

        public IActionResult ReporteAlDia()
        {
            List<Ingeniero> Ingeniero = new List<Ingeniero>();
            Ingeniero = IngenierosAlDia();

            return new ViewAsPdf("ReporteAlDia", Ingeniero)
            {
                FileName = $"AlDia {1}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.Letter
            };
        }



        //Imprimir Recibo Con ItextSharp 
        public ActionResult ImprimirReciboCuotaItext(int? Id)
        {

            //TODO ESTO LO REEMPLAZAS CON TU PROPIA LÓGICA HACIA TU BASE DE DATOS
            ViewModelCuotum cuota = _context.Cuota.Include(dv => dv.DetalleCuota).Where(dv => dv.IdCuota == Id)
                .Select(v => new ViewModelCuotum()
                {
                    IdCuota = v.IdCuota,
                    Fecha = v.Fecha,
                    MesesTotal = v.MesesTotal,
                    Total = v.Total,
                    IdUsuario = v.IdUsuario,
                    IdIngeniero = v.IdIngeniero,
                    IdIngenieroNavigation = v.IdIngenieroNavigation,
                    IdUsuarioNavigation = v.IdUsuarioNavigation,
                    DetalleCuota = v.DetalleCuota.Select(dv => new ViewModelDetalleCuotum()
                    {
                        DetalleMes = dv.DetalleMes,
                        Monto = dv.Monto,
                    }).ToList(),
                }).FirstOrDefault();
            int num = Convert.ToInt32(cuota.Total);
            String numLetras = Literal(num);
            numLetras = numLetras.ToUpper();


            MemoryStream ms = new MemoryStream();

            Document doc = new Document(PageSize.A6, 10, 10, 2, 2);

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

            pdfCell = new PdfPCell(new Paragraph("RECIBO OFICIAL # " + cuota.IdCuota + "\r\n\r\n", fontTextTitleMax));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("RNI: " + cuota.IdIngenieroNavigation.Rni, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("NOMBRE: " + cuota.IdIngenieroNavigation.NombreCompleto, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("ESPECIALIDAD: " + cuota.IdIngenieroNavigation.Especialidad, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Paragraph("______________________________________________________________\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("Por Concepto de: PAGO DE CUOTA", fontText));
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

            int index = 1;
            foreach (var item in cuota.DetalleCuota)
            {
                pdfCell = new PdfPCell(new Paragraph(index.ToString(), fontText));
                pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(pdfCell);

                pdfCell = new PdfPCell(new Paragraph(item.DetalleMes, fontText));
                pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(pdfCell);

                pdfCell = new PdfPCell(new Paragraph(item.Monto.ToString(), fontText));
                pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(pdfCell);

                doc.Add(table);
                table = new PdfPTable(3);
            }



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

            pdfCell = new PdfPCell(new Paragraph(cuota.Total.ToString(), fontTextTitle));
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

            pdfCell = new PdfPCell(new Paragraph(cuota.Total.ToString(), fontTextTitle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);

            doc.Add(cabecera);


            cabecera = new PdfPTable(1);
            pdfCell = new PdfPCell(new Paragraph("______________________________________________________________\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("Son: " + numLetras + " CON 00/100 BOLIVIANOS", fontText));
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

            pdfCell = new PdfPCell(new Paragraph("\r\n", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("Escanee para verificar su pago", fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("Usuario: " + cuota.IdUsuarioNavigation.Cuenta, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            pdfCell = new PdfPCell(new Paragraph("Fecha: " + cuota.Fecha, fontText));
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell.Border = 0;
            cabecera.AddCell(pdfCell);


            doc.Add(cabecera);

            Cuotum cuotum = _context.Cuota.Include(dv => dv.IdIngenieroNavigation).Include(dv => dv.IdUsuarioNavigation).FirstOrDefault(dv => dv.IdCuota == Id);
            String txtQRCode = "RNI: " + cuotum.IdIngenieroNavigation.Rni + ", Nombre: " + cuotum.IdIngenieroNavigation.NombreCompleto.ToString() + " pago de: Cuota de " + cuotum.MesesTotal.ToString()
                + " meses, con un monto de: " + cuotum.Total;
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
