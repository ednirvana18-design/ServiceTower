using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ServiceTowerWeb.Models;
using Rotativa.AspNetCore;

namespace ServiceTowerWeb.Controllers;

public class ExportController : Controller
{
    private readonly string? _connectionString;
    private readonly IWebHostEnvironment _env;

    public ExportController(IConfiguration config, IWebHostEnvironment env)
    {
        _connectionString = config.GetConnectionString("Supabase");
        _env = env;
    }

    // Esta es la ventanita pequeña que se abrirá
    public IActionResult Descargar(string orden, string tipo)
    {
        ViewBag.Orden = orden;
        ViewBag.Tipo = tipo; // Aquí sabremos si quiere CSV o PDF
        return View();
    }

    // Aquí sucede la magia del CSV
    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> GenerarCsv(string orden)
    {
        try
        {
            var builder = new System.Text.StringBuilder();
            // 1. Agregamos los encabezados de las fotos al final
            builder.AppendLine("Orden,Cliente,Tecnico,ID_Equipo,Modelo,Serie,Area,Ubicacion,Etiqueta,Ribbon,Comentarios,FotoAntesUrl,FotoDespuesUrl");

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                // 2. Asegúrate de que el SELECT traiga las columnas de las fotos
                var sql = @"SELECT ""ordenServicio"", ""cliente"", ""tecnico"", ""idEquipo"", ""modelo"", 
                               ""serie"", ""area"", ""ubicacion"", ""usoEtiqueta"", ""usoRibbon"", 
                               ""comentarios"", ""fotoAntesUrl"", ""fotoDespuesUrl"" 
                        FROM mantenimientos 
                        WHERE ""ordenServicio"" ILIKE @o OR ""cliente"" ILIKE @o";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("o", $"%{orden}%");
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var fila = new string[] {
                            reader[0]?.ToString() ?? "",
                            reader[1]?.ToString() ?? "",
                            reader[2]?.ToString() ?? "",
                            reader[3]?.ToString() ?? "",
                            reader[4]?.ToString() ?? "",
                            reader[5]?.ToString() ?? "",
                            reader[6]?.ToString() ?? "",
                            reader[7]?.ToString() ?? "",
                            reader[8]?.ToString() ?? "",
                            reader[9]?.ToString() ?? "",
                            reader[10]?.ToString()?.Replace("\r", " ").Replace("\n", " ").Replace(",", ";") ?? "",
                            // 3. Agregamos los links de las fotos a la fila
                            reader[11]?.ToString() ?? "", // Foto Antes
                            reader[12]?.ToString() ?? ""  // Foto Después
                        };
                            builder.AppendLine(string.Join(",", fila.Select(f => $"\"{f}\"")));
                        }
                    }
                }
            }

            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var fileBytes = bom.Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();

            return File(fileBytes, "text/csv", $"Reporte_{orden}_{DateTime.Now:ddMMyy}.csv");
        }
        catch (Exception ex)
        {
            return Content("Error al generar el archivo con fotos: " + ex.Message);
        }
    }
}