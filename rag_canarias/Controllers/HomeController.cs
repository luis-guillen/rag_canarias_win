using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace rag_canarias.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Crawl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Content("URL vacía");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync(url).ConfigureAwait(false);

                    // Asegurar que la carpeta exista
                    System.IO.Directory.CreateDirectory(@"C:\temp");
                    System.IO.File.WriteAllText(@"C:\temp\pagina.txt", html);

                    return Content("Página descargada y guardada en C:\\temp\\pagina.txt");
                }
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }
    }
}
