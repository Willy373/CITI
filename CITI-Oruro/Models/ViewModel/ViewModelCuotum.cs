namespace CITI_Oruro.Models.ViewModel
{
    public class ViewModelCuotum
    {
        public int IdCuota { get; set; }

        public DateTime Fecha { get; set; }

        public int MesesTotal { get; set; }

        public int? Total { get; set; }

        public int IdUsuario { get; set; }

        public int IdIngeniero { get; set; }

        public virtual Ingeniero? IdIngenieroNavigation { get; set; } 

        public virtual Usuario? IdUsuarioNavigation { get; set; } 


        public List<ViewModelDetalleCuotum> DetalleCuota { get; set; }

        
    }
    public class ViewModelDetalleCuotum
    {
        //public int IdDetalleCuota { get; set; }

        public string DetalleMes { get; set; }

        public int Monto { get; set; }
    }
}
