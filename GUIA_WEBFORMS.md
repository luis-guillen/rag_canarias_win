# 📋 Guía: Cómo usar CrawlerService en Web Forms

## 📦 Archivos a copiar a tu proyecto Web Forms

Copia estos dos archivos a tu proyecto Web Forms:

```
TuProyectoWebForms/
├── Services/
│   ├── CrawlerService.cs        ← COPIAR
│   └── PathHelper.cs            ← COPIAR
└── ... resto del proyecto
```

## 🎯 Paso 1: Agregar dependencia NuGet

Asegúrate de tener en tu proyecto Web Forms:

```
HtmlAgilityPack (version 1.11.x o superior)
```

En Visual Studio:
```
Tools → NuGet Package Manager → Package Manager Console

Install-Package HtmlAgilityPack
```

## 🔧 Paso 2: Uso en Web Forms

### Ejemplo en Default.aspx.cs

```csharp
using System;
using System.IO;
using System.Web.UI;
using rag_canarias.Services;  // ← AGREGAR THIS IMPORT

public partial class Default : Page
{
    protected void BtnCrawl_Click(object sender, EventArgs e)
    {
        try
        {
            // 1. Obtener parámetros del formulario
            string url = txtUrl.Text.Trim();
            int maxPages = int.Parse(txtMaxPages.Text ?? "50");
            int maxDepth = int.Parse(txtMaxDepth.Text ?? "2");
            string carpetaPersonalizada = txtCarpeta.Text.Trim();

            // 2. Resolver ruta de guardado
            string appDataBase = Server.MapPath("~/App_Data/");
            string carpetaGuardado = PathHelper.ResolverRutaCarpeta(appDataBase, carpetaPersonalizada);

            // 3. Crear carpeta para el dominio
            string nombreDominio = new Uri(url).Host.Replace(".", "_");
            string carpetaDominio = Path.Combine(carpetaGuardado, nombreDominio);

            // 4. Ejecutar crawling
            var crawlerService = new CrawlerService();
            var resultado = crawlerService.CrawlDominio(url, carpetaDominio, maxPages, maxDepth);

            // 5. Mostrar resultados
            if (resultado.Exitoso)
            {
                lblResultado.Text = $"✓ {resultado.Mensaje}";
                lblResultado.ForeColor = System.Drawing.Color.Green;

                // Mostrar ruta relativa
                string rutaRelativa = PathHelper.ObtenerRutaRelativa(
                    Server.MapPath("~"), 
                    resultado.RutaRelativa
                );
                lblRuta.Text = $"Archivos guardados en: {rutaRelativa}";
            }
            else
            {
                lblResultado.Text = $"✗ Error: {resultado.Mensaje}";
                lblResultado.ForeColor = System.Drawing.Color.Red;
            }
        }
        catch (Exception ex)
        {
            lblResultado.Text = $"✗ Error inesperado: {ex.Message}";
            lblResultado.ForeColor = System.Drawing.Color.Red;
        }
    }
}
```

## 📝 Ejemplo en ASPX (Default.aspx)

```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="TuProyecto.Default" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Crawler Canarias</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .form-group { margin-bottom: 15px; }
        input, select { padding: 8px; width: 300px; }
        button { padding: 10px 20px; background-color: #007bff; color: white; border: none; cursor: pointer; }
        button:hover { background-color: #0056b3; }
        .message { margin-top: 20px; padding: 10px; border-radius: 4px; }
    </style>
</head>
<body>
    <h2>Crawler Patrimonio Canarias</h2>

    <form id="form1" runat="server">
        <div class="form-group">
            <label>URL a rastrear:</label>
            <asp:TextBox ID="txtUrl" runat="server" TextMode="Url" />
        </div>

        <div class="form-group">
            <label>Máximo de páginas:</label>
            <asp:TextBox ID="txtMaxPages" runat="server" TextMode="Number" Text="50" />
        </div>

        <div class="form-group">
            <label>Profundidad máxima:</label>
            <asp:TextBox ID="txtMaxDepth" runat="server" TextMode="Number" Text="2" />
        </div>

        <div class="form-group">
            <label>Carpeta de guardado (dentro de App_Data):</label>
            <asp:TextBox ID="txtCarpeta" runat="server" Placeholder="dejar vacío para 'crawlings'" />
        </div>

        <asp:Button ID="BtnCrawl" runat="server" Text="Iniciar Crawling" OnClick="BtnCrawl_Click" />

        <div class="message">
            <asp:Label ID="lblResultado" runat="server" />
            <br />
            <asp:Label ID="lblRuta" runat="server" />
        </div>
    </form>
</body>
</html>
```

## 🚀 Opción avanzada: Crear un modelo de resultado

Si quieres usar la clase de resultado en múltiples lugares:

```csharp
// En Services/CrawlerResult.cs
public class CrawlerResult
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; }
    public int PaginasDescargadas { get; set; }
    public string RutaAbsoluta { get; set; }
    public string RutaRelativa { get; set; }
    public Exception Excepcion { get; set; }
}

// Uso en Default.aspx.cs
var resultado = crawlerService.CrawlDominio(url, carpetaDominio, maxPages, maxDepth);
// Resultado es automáticamente de tipo CrawlerResult
```

## ⚙️ Configuración recomendada

### Web.config - No se necesita configuración especial

El `Server.MapPath()` funciona automáticamente en Web Forms igual que en MVC.

## ✅ Checklist

- [ ] Archivos `CrawlerService.cs` y `PathHelper.cs` copiados a carpeta `Services/`
- [ ] `HtmlAgilityPack` instalado vía NuGet
- [ ] Importes añadidos (`using rag_canarias.Services;`)
- [ ] Formulario ASPX creado con los controles necesarios
- [ ] Code-behind del formulario implementado
- [ ] Carpeta `App_Data/` existe en el proyecto Web Forms
- [ ] `.gitignore` incluye `App_Data/crawlings/*` (para no versionar descargas)

## 🔗 Referencias

- **CrawlerService.CrawlDominio()**: Método principal que hace todo el crawling
  - Retorna `ResultadoCrawl` con info del resultado
  - Maneja excepciones automáticamente
  
- **CrawlerService.ExtraerTextoLimpio()**: Método público para limpiar HTML
  - Útil si quieres procesar HTML adicional
  
- **PathHelper.ResolverRutaCarpeta()**: Resuelve rutas con validación
  - Rechaza intentos de path traversal
  - Siempre guarda en App_Data
  
- **PathHelper.ObtenerRutaRelativa()**: Convierte rutas absolutas a relativas
  - Formato: `[Proyecto]/App_Data/crawlings/...`

## 💡 Tips

1. **Para múltiples URLs**: Crea un loop igual que en MVC:
```csharp
string[] urls = { "https://sitio1.com", "https://sitio2.com" };
foreach (var url in urls)
{
    var resultado = crawlerService.CrawlDominio(url, carpetaBase, maxPages, maxDepth);
    // Procesar resultado...
}
```

2. **Para guardar historial**: Usa una tabla SQL o archivo JSON:
```csharp
// Guardar en BD
var log = new CrawlLog 
{ 
    Url = url, 
    FechaExecution = DateTime.Now, 
    PaginasDescargadas = resultado.PaginasDescargadas 
};
// dbContext.CrawlLogs.Add(log);
```

3. **Para monitoreo**: Implementa logging:
```csharp
System.Diagnostics.Debug.WriteLine($"Crawling: {url} → {resultado.PaginasDescargadas} páginas");
```
