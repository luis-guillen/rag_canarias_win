using HtmlAgilityPack;
using System;
using System.IO;
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

        [HttpPost]
        public ActionResult Crawl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Content("URL vacía");
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return Content("URL no válida");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var html = client.GetStringAsync(url).Result;
                    var textoLimpio = ExtraerTextoLimpio(html);

                    var carpeta = @"C:\temp";
                    Directory.CreateDirectory(carpeta);

                    var rutaArchivo = Path.Combine(carpeta, "pagina_limpia.txt");
                    System.IO.File.WriteAllText(rutaArchivo, textoLimpio, Encoding.UTF8);

                    return Content("Texto limpio guardado en: " + rutaArchivo);
                }
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
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
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();
            foreach (var linea in lineas)
            {
                var limpia = linea.Trim();
                if (!string.IsNullOrWhiteSpace(limpia))
                {
                    sb.AppendLine(limpia);
                }
            }

            return sb.ToString();
        }
    }
}