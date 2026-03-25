using System;
using System.Text.Json.Serialization;

namespace ServiceTowerWeb.Models
{
    public class Mantenimiento
    {
        public int Id { get; set; }

        [JsonPropertyName("tecnico")]
        public string? Tecnico { get; set; }

        [JsonPropertyName("cliente")]
        public string? Cliente { get; set; }

        [JsonPropertyName("ordenServicio")]
        public string? OrdenServicio { get; set; }

        [JsonPropertyName("idEquipo")]
        public string? IdEquipo { get; set; }

        [JsonPropertyName("modelo")]
        public string? Modelo { get; set; }

        [JsonPropertyName("serie")]
        public string? Serie { get; set; }

        [JsonPropertyName("resolucion")]
        public string? Resolucion { get; set; }

        [JsonPropertyName("area")]
        public string? Area { get; set; }

        [JsonPropertyName("ubicacion")]
        public string? Ubicacion { get; set; }

        [JsonPropertyName("comentarios")]
        public string? Comentarios { get; set; }

        [JsonPropertyName("usoEtiqueta")]
        public string? UsoEtiqueta { get; set; }

        [JsonPropertyName("usoRibbon")]
        public string? UsoRibbon { get; set; }

        [JsonPropertyName("fecha")]
        public long Fecha { get; set; } // Cambiado a long para compatibilidad con la App

        [JsonPropertyName("fotoAntesUrl")]
        public string? FotoAntesUrl { get; set; }

        [JsonPropertyName("fotoDespuesUrl")]
        public string? FotoDespuesUrl { get; set; }
    }
}