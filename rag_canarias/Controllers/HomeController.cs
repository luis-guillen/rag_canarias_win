using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
        public ActionResult Crawl(string url, int maxPages = 50, int maxDepth = 2, bool fullCrawl = false, string carpetaGuardado = "")
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
            // Validar y aplicar parámetros
            if (!string.IsNullOrWhiteSpace(url))
            {
                // Si se envía una URL desde la UI, usarla como única semilla
                seeds = new List<string> { url };
            }

            // Asegurar valores mínimos
            maxPages = Math.Max(1, maxPages);
            maxDepth = Math.Max(0, maxDepth);

            // Si FullCrawl está activo, usar 1000 fijo; si no, respetar maxPages
            int maxPaginasPorSitio = fullCrawl ? 1000 : Math.Max(1, maxPages);

            // Resolver ruta de guardado
            string carpetaBaseGlobal = ResolverRutaCarpeta(carpetaGuardado);

            try
            {
                Directory.CreateDirectory(carpetaBaseGlobal);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al crear carpeta: {ex.Message}";
                return View("Resultados");
            }

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

                    int total = CrawlDomain(startUri, maxPaginasPorSitio, maxDepth, carpetaSitio);

                    // Mostrar ruta relativa al proyecto para mayor claridad
                    string rutaRelativa = ObtenerRutaRelativa(carpetaSitio);
                    resultados.Add($"✓ {startUri.Host} → {total} páginas en {rutaRelativa}");
                }
                catch (Exception ex)
                {
                    resultados.Add($"✗ {seed} → ERROR: {ex.Message}");
                }
            }

            ViewBag.Resultados = resultados;
            ViewBag.CarpetaBase = ObtenerRutaRelativa(carpetaBaseGlobal);
            return View("Resultados");
        }

        private int CrawlDomain(Uri startUri, int maxPaginas, int maxDepth, string carpetaBase)
        {
            var visitadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var cola = new Queue<Tuple<Uri, int>>(); // Uri + depth
            cola.Enqueue(Tuple.Create(startUri, 0));

            int contador = 0;

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("TFG-Crawler/1.0");

                while (cola.Count > 0 && contador < maxPaginas)
                {
                    var item = cola.Dequeue();
                    var currentUri = item.Item1;
                    int depth = item.Item2;

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
                        // pequeña espera para no sobrecargar en fallos
                        System.Threading.Thread.Sleep(300);
                        continue;
                    }

                    string textoLimpio = ExtraerTextoLimpio(html);

                    if (string.IsNullOrWhiteSpace(textoLimpio))
                        continue;

                    string nombreArchivo = GenerarNombreSeguro(currentUri, contador + 1);
                    string rutaArchivo = Path.Combine(carpetaBase, nombreArchivo);

                    System.IO.File.WriteAllText(rutaArchivo, textoLimpio, Encoding.UTF8);
                    contador++;

                    // Añadir delay entre peticiones (politeness)
                    System.Threading.Thread.Sleep(300);

                    if (depth < maxDepth)
                    {
                        var enlaces = ExtraerEnlacesInternos(html, currentUri, startUri.Host);

                        foreach (var enlace in enlaces)
                        {
                            string enlaceNormalizado = NormalizarUrl(enlace);

                            if (!visitadas.Contains(enlaceNormalizado))
                            {
                                cola.Enqueue(Tuple.Create(enlace, depth + 1));
                            }
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

        private static readonly string[] _nodosBasura =
        {
            "script", "style", "noscript", "nav", "header", "footer", "aside", "form"
        };

        private static readonly string[] _nodosUtiles =
        {
            "h1", "h2", "h3", "h4", "h5", "h6", "p", "li", "blockquote"
        };

        private static readonly string[] _patronesRuido =
        {
            "aviso legal", "política de privacidad", "política de cookies", "uso de cookies",
            "contacto", "teléfono", "correo electrónico", "compartir", "enviar comentario",
            "suscríbete", "síguenos", "redes sociales", "todos los derechos reservados",
            "copyright", "newsletter", "iniciar sesión", "cerrar sesión", "registrar",
            "politica de privacidad", "politica de cookies",
            "telefono", "correo electronico",
            "suscribete", "siguenos",
            "iniciar sesion", "cerrar sesion"
        };

        private string ExtraerTextoLimpio(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 1. Eliminar nodos de ruido estructural
            var xpathBasura = string.Join("|", _nodosBasura.Select(t => "//" + t));
            var nodosBasura = doc.DocumentNode.SelectNodes(xpathBasura);
            if (nodosBasura != null)
            {
                // ToList() evita modificar la colección mientras se itera
                foreach (var nodo in nodosBasura.ToList())
                    nodo.Remove();
            }

            // 2. Detectar zona de contenido principal
            HtmlNode contenido =
                doc.DocumentNode.SelectSingleNode("//main") ??
                doc.DocumentNode.SelectSingleNode("//article") ??
                doc.DocumentNode.SelectSingleNode("//body") ??
                doc.DocumentNode;

            // 3. Extraer sólo nodos semánticamente útiles
            var xpathUtiles = string.Join("|", _nodosUtiles.Select(t => "descendant::" + t));
            var nodosUtiles = contenido.SelectNodes(xpathUtiles);

            if (nodosUtiles == null || nodosUtiles.Count == 0)
            {
                // Fallback: texto plano del contenido limpio
                nodosUtiles = new HtmlNodeCollection(contenido) { contenido };
            }

            // 4. Construir líneas de texto
            var sb = new StringBuilder();
            foreach (var nodo in nodosUtiles)
            {
                string texto = HtmlEntity.DeEntitize(nodo.InnerText);
                // Normalizar espacios internos
                texto = Regex.Replace(texto, @"\s+", " ").Trim();

                if (!string.IsNullOrWhiteSpace(texto))
                    sb.AppendLine(texto);
            }

            // 5. Filtrar línea a línea
            var líneas = sb.ToString()
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => l.Length >= 30)
                .Where(l => !_patronesRuido.Any(p =>
                    l.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0));

            return string.Join(Environment.NewLine, líneas);
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

        /// <summary>
        /// Resuelve la ruta de guardado. Si está vacía, usa App_Data/crawlings.
        /// Si es una ruta relativa, la resuelve desde el raíz del proyecto.
        /// </summary>
        private string ResolverRutaCarpeta(string carpetaPersonalizada)
        {
            if (string.IsNullOrWhiteSpace(carpetaPersonalizada))
            {
                // Ruta por defecto: App_Data/crawlings dentro del proyecto
                return Server.MapPath("~/App_Data/crawlings/");
            }

            // Limpiar la ruta de barras extras
            carpetaPersonalizada = carpetaPersonalizada.Trim().Trim('/').Trim('\\');

            // Si es una ruta relativa (no comienza con / ni \ ni contiene :)
            if (!carpetaPersonalizada.Contains(":") && 
                !carpetaPersonalizada.StartsWith("/") && 
                !carpetaPersonalizada.StartsWith("\\"))
            {
                // Resolver como ruta relativa desde raíz del proyecto
                return Server.MapPath($"~/{carpetaPersonalizada}/");
            }

            // Si es una ruta absoluta, usarla tal cual
            return carpetaPersonalizada.EndsWith("\\") ? carpetaPersonalizada : carpetaPersonalizada + "\\";
        }

        /// <summary>
        /// Obtiene la ruta relativa al proyecto para mostrar al usuario.
        /// </summary>
        private string ObtenerRutaRelativa(string rutaAbsoluta)
        {
            try
            {
                string raizProyecto = Server.MapPath("~");
                if (rutaAbsoluta.StartsWith(raizProyecto, StringComparison.OrdinalIgnoreCase))
                {
                    // Extraer ruta relativa
                    string relativa = rutaAbsoluta.Substring(raizProyecto.Length).Trim('\\').Trim('/');
                    return $"[Proyecto]/{relativa}";
                }
            }
            catch { }

            // Si no se puede determinar, devolver la ruta tal cual
            return rutaAbsoluta;
        }
    }
}