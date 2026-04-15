using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace rag_canarias.Services
{
    /// <summary>
    /// Servicio de crawling independiente de MVC/Web Forms.
    /// Reutilizable en cualquier proyecto .NET Framework 4.8.1+
    /// </summary>
    public class CrawlerService
    {
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

        /// <summary>
        /// Resultado del crawling con información estructurada
        /// </summary>
        public class ResultadoCrawl
        {
            public bool Exitoso { get; set; }
            public string Mensaje { get; set; }
            public int PaginasDescargadas { get; set; }
            public string RutaRelativa { get; set; }
            public Exception Excepcion { get; set; }
        }

        /// <summary>
        /// Realiza crawling de un dominio completo
        /// </summary>
        /// <param name="urlSemilla">URL inicial a rastrear (ej: https://ejemplo.com/)</param>
        /// <param name="carpetaGuardado">Ruta absoluta donde guardar los archivos</param>
        /// <param name="maxPaginas">Máximo de páginas a descargar (default 50)</param>
        /// <param name="maxDepth">Profundidad máxima de enlaces a seguir (default 2)</param>
        /// <returns>ResultadoCrawl con información del resultado</returns>
        public ResultadoCrawl CrawlDominio(string urlSemilla, string carpetaGuardado, int maxPaginas = 50, int maxDepth = 2)
        {
            var resultado = new ResultadoCrawl();

            try
            {
                if (!Uri.TryCreate(urlSemilla, UriKind.Absolute, out Uri startUri))
                {
                    resultado.Exitoso = false;
                    resultado.Mensaje = $"URL inválida: {urlSemilla}";
                    return resultado;
                }

                // Crear carpeta si no existe
                try
                {
                    Directory.CreateDirectory(carpetaGuardado);
                    if (!Directory.Exists(carpetaGuardado))
                    {
                        throw new Exception($"No se pudo crear la carpeta: {carpetaGuardado}");
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    resultado.Exitoso = false;
                    resultado.Mensaje = $"Permiso denegado al crear carpeta: {ex.Message}";
                    resultado.Excepcion = ex;
                    return resultado;
                }
                catch (ArgumentException ex)
                {
                    resultado.Exitoso = false;
                    resultado.Mensaje = $"Ruta inválida: {ex.Message}";
                    resultado.Excepcion = ex;
                    return resultado;
                }

                // Ejecutar crawl
                int totalDescargadas = EjecutarCrawl(startUri, maxPaginas, maxDepth, carpetaGuardado);

                resultado.Exitoso = true;
                resultado.PaginasDescargadas = totalDescargadas;
                resultado.RutaRelativa = carpetaGuardado;
                resultado.Mensaje = $"Crawling completado: {totalDescargadas} páginas descargadas";
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error durante el crawling: {ex.Message}";
                resultado.Excepcion = ex;
            }

            return resultado;
        }

        /// <summary>
        /// Ejecuta el crawling en BFS
        /// </summary>
        private int EjecutarCrawl(Uri startUri, int maxPaginas, int maxDepth, string carpetaBase)
        {
            var visitadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var cola = new Queue<Tuple<Uri, int>>();
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
                        System.Threading.Thread.Sleep(300);
                        continue;
                    }

                    string textoLimpio = ExtraerTextoLimpio(html);

                    if (string.IsNullOrWhiteSpace(textoLimpio))
                        continue;

                    string nombreArchivo = GenerarNombreSeguro(currentUri, contador + 1);
                    string rutaArchivo = Path.Combine(carpetaBase, nombreArchivo);

                    File.WriteAllText(rutaArchivo, textoLimpio, Encoding.UTF8);
                    contador++;

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

        /// <summary>
        /// Extrae texto limpio del HTML con filtrado inteligente
        /// </summary>
        public string ExtraerTextoLimpio(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 1. Eliminar nodos de ruido estructural
            var xpathBasura = string.Join("|", _nodosBasura.Select(t => "//" + t));
            var nodosBasura = doc.DocumentNode.SelectNodes(xpathBasura);
            if (nodosBasura != null)
            {
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
                nodosUtiles = new HtmlNodeCollection(contenido) { contenido };
            }

            // 4. Construir líneas de texto
            var sb = new StringBuilder();
            foreach (var nodo in nodosUtiles)
            {
                string texto = HtmlEntity.DeEntitize(nodo.InnerText);
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

        public string GenerarNombreCarpetaDominio(Uri uri)
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
