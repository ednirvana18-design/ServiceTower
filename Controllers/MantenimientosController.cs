using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceTowerWeb.Models;

public class MantenimientosController : Controller
{
    private readonly string _connectionString;

    public MantenimientosController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Supabase");
    }

    // 🔹 INDEX CON FILTRO DESDE BASE DE DATOS
    public async Task<IActionResult> Index(string orden)
    {
        var lista = new List<Mantenimiento>();

        await using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            var query = "SELECT * FROM mantenimientos";

            if (!string.IsNullOrEmpty(orden))
            {
                query += " WHERE \"ordenServicio\" = @orden";
            }

            query += " ORDER BY id DESC"; // 🔥 importante

            await using (var cmd = new NpgsqlCommand(query, conn))
            {
                if (!string.IsNullOrEmpty(orden))
                {
                    cmd.Parameters.AddWithValue("orden", orden);
                }

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Mantenimiento
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            OrdenServicio = reader["ordenServicio"]?.ToString(),
                            Modelo = reader["modelo"]?.ToString(),
                            Serie = reader["serie"]?.ToString(),
                            Area = reader["area"]?.ToString(),
                            Comentarios = reader["comentarios"]?.ToString(),
                            FotoAntesUrl = reader["fotoantesurl"]?.ToString(),
                            FotoDespuesUrl = reader["fotodespuesurl"]?.ToString()
                        });
                    }
                }
            }
        }

        return View(lista);
    }

    // 🔹 PDF (por ahora simple, luego lo hacemos pro)
    public async Task<IActionResult> GenerarPdf(string orden)
    {
        var lista = new List<Mantenimiento>();

        await using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            var query = "SELECT * FROM mantenimientos";

            if (!string.IsNullOrEmpty(orden))
            {
                query += " WHERE \"ordenServicio\" = @orden";
            }

            await using (var cmd = new NpgsqlCommand(query, conn))
            {
                if (!string.IsNullOrEmpty(orden))
                {
                    cmd.Parameters.AddWithValue("orden", orden);
                }

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Mantenimiento
                        {
                            OrdenServicio = reader["ordenServicio"]?.ToString(),
                            Modelo = reader["modelo"]?.ToString(),
                            Serie = reader["serie"]?.ToString(),
                            Area = reader["area"]?.ToString(),
                            Comentarios = reader["comentarios"]?.ToString()
                        });
                    }
                }
            }
        }

        // 🔥 TEMPORAL (luego hacemos PDF real)
        return Content($"PDF generado con {lista.Count} registros de la OS: {orden}");
    }
}