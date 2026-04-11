using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;

namespace rag_canarias.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        // Acción GET para poder navegar directamente a /Home/Resultados
        public ActionResult Resultados()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Crawl()
        {
            var seeds = new List<string>
            {
                "https://elmuseocanario.com/",
                "https://cultura.grancanaria.com/museos",
                "https://memoriadelanzarote.com/",
                "https://canarias-azul.iatext.ulpgc.es/",
                "https://izuran.blogspot.com/",
                "https://www.academiacanarialengua.org/diccionario/"
            };

            // Sin límite: rastrear todas las páginas encontradas en el dominio
            int maxPaginasPorSitio = int.MaxValue;
            string carpetaBaseGlobal = @"C:\temp\crawler";

            Directory.CreateDirectory(carpetaBaseGlobal);

            var resultados = new List<string>();

            foreach (var seed in seeds)
            {
                try
                {
                    if (!Uri.TryCreate(seed, UriKind.Absolute, out Uri startUri))
                    {
                        resultados.Add($"URL inválida: {seed}");
                        continue;
                    }

                    string nombreCarpeta = GenerarNombreCarpetaDominio(startUri);
                    string carpetaSitio = Path.Combine(carpetaBaseGlobal, nombreCarpeta);
                    Directory.CreateDirectory(carpetaSitio);

                    int total = CrawlDomain(startUri, maxPaginasPorSitio, carpetaSitio);

                    resultados.Add($"{startUri.Host} -> {total} páginas guardadas en {carpetaSitio}");
                }
                catch (Exception ex)
                {
                    resultados.Add($"{seed} -> ERROR: {ex.Message}");
                }
            }

            ViewBag.Resultados = resultados;
            return View("Resultados");
        }

        private int CrawlDomain(Uri startUri, int maxPaginas, string carpetaBase)
        {
            var visitadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var cola = new Queue<Uri>();
            cola.Enqueue(startUri);

            int contador = 0;

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("TFG-Crawler/1.0");

                while (cola.Count > 0 && contador < maxPaginas)
                {
                    var currentUri = cola.Dequeue();
                    string currentUrl = NormalizarUrl(currentUri);

                    if (visitadas.Contains(currentUrl))
                        continue;

                    visitadas.Add(currentUrl);

                    string html;
                    try
                    {
                        html = client.GetStringAsync(currentUri).Result;
                    }
                    catch
                    {
                        continue;
                    }

                    string textoLimpio = ExtraerTextoLimpio(html);

                    if (string.IsNullOrWhiteSpace(textoLimpio))
                        continue;

                    string nombreArchivo = GenerarNombreSeguro(currentUri, contador + 1);
                    string rutaArchivo = Path.Combine(carpetaBase, nombreArchivo);

                    System.IO.File.WriteAllText(rutaArchivo, textoLimpio, Encoding.UTF8);
                    contador++;

                    var enlaces = ExtraerEnlacesInternos(html, currentUri, startUri.Host);

                    foreach (var enlace in enlaces)
                    {
                        string enlaceNormalizado = NormalizarUrl(enlace);

                        if (!visitadas.Contains(enlaceNormalizado))
                        {
                            cola.Enqueue(enlace);
                        }
                    }
                }
            }

            return contador;
        }

        private List<Uri> ExtraerEnlacesInternos(string html, Uri baseUri, string hostObjetivo)
        {
            var resultado = new List<Uri>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var links = doc.DocumentNode.SelectNodes("//a[@href]");
            if (links == null)
                return resultado;

            foreach (var link in links)
            {
                var href = link.GetAttributeValue("href", "").Trim();

                if (string.IsNullOrWhiteSpace(href))
                    continue;

                if (href.StartsWith("#") ||
                    href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
                    href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
                    href.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (Uri.TryCreate(baseUri, href, out Uri nuevaUri))
                {
                    if (!EsUrlRastreable(nuevaUri))
                        continue;

                    if (string.Equals(nuevaUri.Host, hostObjetivo, StringComparison.OrdinalIgnoreCase))
                    {
                        resultado.Add(nuevaUri);
                    }
                }
            }

            return resultado;
        }

        private bool EsUrlRastreable(Uri uri)
        {
            if (!(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                return false;

            string path = uri.AbsolutePath.ToLowerInvariant();

            string[] extensionesNoDeseadas =
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg",
                ".pdf", ".zip", ".rar", ".7z",
                ".mp4", ".mp3", ".wav",
                ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx"
            };

            return !extensionesNoDeseadas.Any(ext => path.EndsWith(ext));
        }

        private string ExtraerTextoLimpio(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var basura = doc.DocumentNode.SelectNodes("//script|//style|//noscript");
            if (basura != null)
            {
                foreach (var nodo in basura)
                {
                    nodo.Remove();
                }
            }

            var texto = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);

            var lineas = texto
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l));

            return string.Join(Environment.NewLine, lineas);
        }

        private string GenerarNombreSeguro(Uri uri, int numero)
        {
            string path = uri.AbsolutePath.Trim('/');

            if (string.IsNullOrWhiteSpace(path))
                path = "home";

            path = path.Replace("/", "_");

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                path = path.Replace(c, '_');
            }

            if (path.Length > 80)
                path = path.Substring(0, 80);

            return $"{numero:D2}_{path}.txt";
        }

        private string NormalizarUrl(Uri uri)
        {
            var builder = new UriBuilder(uri)
            {
                Fragment = ""
            };

            string url = builder.Uri.ToString().TrimEnd('/');

            return url;
        }

        private string GenerarNombreCarpetaDominio(Uri uri)
        {
            string nombre = uri.Host.Replace(".", "_");
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                nombre = nombre.Replace(c, '_');
            }
            return nombre;
        }
    }
}