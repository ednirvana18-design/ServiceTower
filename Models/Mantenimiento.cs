namespace ServiceTowerWeb.Models
{
    public class Mantenimiento
    {
        public int Id { get; set; }
        public string OrdenServicio { get; set; }
        public string Modelo { get; set; }
        public string Serie { get; set; }
        public string Area { get; set; }
        public string Comentarios { get; set; }

        public string FotoAntesUrl { get; set; }
        public string FotoDespuesUrl { get; set; }
    }
}