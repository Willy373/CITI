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
                return "\n\tIngresar valores menores a 10000\n";
        }



        public async Task<IActionResult> ImprimirCuota(int? Id)
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
                        //IdDetalleCuota = dv.IdDetalleCuota,
                        DetalleMes = dv.DetalleMes,
                        Monto = dv.Monto,
                    }).ToList(),
                }).FirstOrDefault();
                int num = Convert.ToInt32(cuota.Total);
                String numLetras = Literal(num);
                cuota.IdUsuarioNavigation.Password = numLetras.ToUpper();



            //String txtQRCode = detalle.IdIngenieroNavigation.NombreCompleto.ToString() + " pago de: " + detalle.IdInscripcionNavigation.Tipo
            //   + ", con un monto de:" + detalle.Total;

            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
            //QRCode qrCode = new QRCode(qrCodeData);
            //Bitmap qrcodeImage = qrCode.GetGraphic(20);

            //ImageConverter converter = new ImageConverter();
            //byte[] QrByte = (byte[])converter.ConvertTo(qrcodeImage, typeof(byte[]));
            //string model = Convert.ToBase64String(QrByte);


            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
            //PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            //byte[] qrCodeImage = qrCode.GetGraphic(20);
            //string model = Convert.ToBase64String(qrCodeImage);


            return new ViewAsPdf("ReciboCuota", cuota)
            {
                FileName = $"Recibo {Id}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A5
            };
        }
    }
}
