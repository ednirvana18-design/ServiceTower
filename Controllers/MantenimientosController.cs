using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using ServiceTowerWeb.Models;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ServiceTowerWeb.Controllers;

[Authorize]
public class MantenimientosController : Controller
{
    private readonly string? _connectionString;
    private readonly IWebHostEnvironment _env;

    public MantenimientosController(IConfiguration config, IWebHostEnvironment env)
    {
        _connectionString = config.GetConnectionString("Supabase");
        _env = env;
    }

    public async Task<IActionResult> Index(string orden, int pagina = 1)
    {
        var lista = await ObtenerDataCompleta(orden, pagina, 50);
        ViewBag.EsCliente = false;
        ViewBag.Orden = orden;
        return View(lista);
    }

    // ACCIÓN PARA EL LINK DEL CLIENTE (PÚBLICO)
    [AllowAnonymous]
    public async Task<IActionResult> Compartir(string orden)
    {
        var lista = await ObtenerDataCompleta(orden, 1, 100);
        ViewBag.EsCliente = true;
        ViewBag.Orden = orden;
        return View("Index", lista);
    }

    public async Task<IActionResult> ReportePdf(string orden)
    {
        try
        {
            // 1. Traemos la data filtrada (usamos 500 por si la orden es muy grande)
            var dataOriginal = await ObtenerDataCompleta(orden, 1, 500);

            if (dataOriginal == null || !dataOriginal.Any())
                return Content("No se encontraron datos para generar el PDF.");

            // 2. IMPORTANTE: Necesitamos las fotos para el PDF. 
            // Si no las incluyes aquí, el PDF saldrá vacío en la sección de evidencias.
            var listaParaPdf = dataOriginal.Select(m => new Mantenimiento
            {
                IdEquipo = m.IdEquipo,
                OrdenServicio = m.OrdenServicio,
                Tecnico = m.Tecnico,
                Cliente = m.Cliente,
                Modelo = m.Modelo,
                Serie = m.Serie,
                Area = m.Area,
                Ubicacion = m.Ubicacion,
                UsoEtiqueta = m.UsoEtiqueta,
                UsoRibbon = m.UsoRibbon,
                Comentarios = m.Comentarios,
                FotoAntesUrl = m.FotoAntesUrl, // ¡No las quites!
                FotoDespuesUrl = m.FotoDespuesUrl, // ¡No las quites!
                Fecha = m.Fecha
            }).ToList();

            // 3. Pasamos datos extra a la vista del PDF (Logo y texto de búsqueda)
            ViewData["Orden"] = string.IsNullOrEmpty(orden) ? "General" : orden;
            ViewData["RootPath"] = _env.WebRootPath;

            // 4. Configuración de Rotativa
            return new ViewAsPdf("ReportePdf", listaParaPdf) // Asegúrate que el nombre coincida con tu .cshtml
            {
                FileName = $"Reporte_IRASA_{orden}_{DateTime.Now:ddMMyy}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--enable-local-file-access" // QUITAMOS --no-images para que se vean las fotos
            };
        }
        catch (Exception ex)
        {
            return Content("Error al generar PDF: " + ex.Message);
        }
    }

    private async Task<List<Mantenimiento>> ObtenerDataCompleta(string filtro, int pagina, int cantidad)
    {
        var lista = new List<Mantenimiento>();
        int offset = (pagina - 1) * cantidad;
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = @"SELECT * FROM mantenimientos ";
        if (!string.IsNullOrEmpty(filtro))
        {
            sql += @" WHERE ""ordenServicio"" ILIKE @f 
                  OR ""serie"" ILIKE @f 
                  OR ""cliente"" ILIKE @f 
                  OR ""idEquipo"" ILIKE @f";
        }
        sql += " ORDER BY id DESC LIMIT @l OFFSET @of";

        await using var cmd = new NpgsqlCommand(sql, conn);
        if (!string.IsNullOrEmpty(filtro)) cmd.Parameters.AddWithValue("f", $"%{filtro}%");
        cmd.Parameters.AddWithValue("l", cantidad);
        cmd.Parameters.AddWithValue("of", offset);

        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            lista.Add(new Mantenimiento
            {
                IdEquipo = r["idEquipo"]?.ToString(),
                OrdenServicio = r["ordenServicio"]?.ToString(),
                Tecnico = r["tecnico"]?.ToString(),
                Cliente = r["cliente"]?.ToString(),
                UsoEtiqueta = r["usoEtiqueta"]?.ToString(),
                UsoRibbon = r["usoRibbon"]?.ToString(),
                Modelo = r["modelo"]?.ToString(),
                Serie = r["serie"]?.ToString(),
                Resolucion = r["resolucion"]?.ToString(),
                Area = r["area"]?.ToString(),
                Ubicacion = r["ubicacion"]?.ToString(),
                Comentarios = r["comentarios"]?.ToString(),
                FotoAntesUrl = r["fotoAntesUrl"]?.ToString(),
                FotoDespuesUrl = r["fotoDespuesUrl"]?.ToString(),
                Fecha = r["fecha"] != DBNull.Value ? Convert.ToInt64(r["fecha"]) : 0
            });
        }
        return lista;
    }

    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> DescargarCsv(string orden)
    {
        try
        {
            // Usamos tu método original tal cual estaba
            var reportes = await ObtenerDataCompleta(orden, 1, 100);

            if (reportes == null || !reportes.Any())
                return Content("No hay datos para esta búsqueda.");

            var builder = new System.Text.StringBuilder();
            // Encabezados exactos
            builder.AppendLine("Orden,Cliente,Tecnico,ID_Equipo,Modelo,Serie,Area,Ubicacion,Etiqueta,Ribbon,Comentarios,FotoAntes,FotoDespues");

            foreach (var r in reportes)
            {
                // Limpieza básica para no romper las celdas
                string c = r.Comentarios?.Replace("\r", " ").Replace("\n", " ").Replace(",", ";") ?? "";

                builder.AppendLine($"\"{r.OrdenServicio}\",\"{r.Cliente}\",\"{r.Tecnico}\",\"{r.IdEquipo}\",\"{r.Modelo}\",\"{r.Serie}\",\"{r.Area}\",\"{r.Ubicacion}\",\"{r.UsoEtiqueta}\",\"{r.UsoRibbon}\",\"{c}\",\"{r.FotoAntesUrl}\",\"{r.FotoDespuesUrl}\"");
            }

            byte[] bom = { 0xEF, 0xBB, 0xBF };
            byte[] d = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            return File(bom.Concat(d).ToArray(), "text/csv", $"Reporte_IRASA_{DateTime.Now:ddMMyy}.csv");
        }
        catch (Exception ex)
        {
            return Content("Error: " + ex.Message);
        }
    }
}