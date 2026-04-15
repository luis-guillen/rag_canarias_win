# 🎁 EXTRACCIÓN LISTA PARA COPY-PASTE

## 📋 LA FORMA MÁS RÁPIDA: 3 opciones

---

## **OPCIÓN 1: Automatizada (1 minuto)** ⚡

```powershell
# Abre PowerShell en la carpeta del repo MVC y ejecuta:
.\Exportar-A-WebForms.ps1 -DestinationProject "D:\TuProyecto\WebFormsApp"
```

✅ Copia archivos
✅ Crea carpetas
✅ Valida todo
✅ Listo en 1 minuto

---

## **OPCIÓN 2: Manual (2 minutos)** 🎯

### Paso 1: Copiar archivos
```
COPIAR DESDE:
  C:\Users\Luis\source\repos\rag_canarias\rag_canarias\Services\

A:
  [TU_WEBFORMS]\[TU_PROYECTO]\Services\

ARCHIVOS:
  ✅ CrawlerService.cs
  ✅ PathHelper.cs
```

### Paso 2: Crear carpetas
```
En tu proyecto Web Forms, crear:
  📁 App_Data\
  📁 Services\  (pegar archivos aquí)
```

### Paso 3: Instalar NuGet
```
Tools → NuGet Package Manager → Package Manager Console
Install-Package HtmlAgilityPack
```

### Paso 4: Usar en code-behind
```csharp
using [TuNamespace].Services;

// En tu Button_Click event:
var crawler = new CrawlerService();
var resultado = crawler.CrawlDominio(url, carpeta);
```

---

## **OPCIÓN 3: Git Subtree** (para repos Git)

```bash
# Desde tu repo Web Forms:
git subtree add --prefix=Services https://github.com/luis-guillen/rag_canarias_win.git crawler-filtrado:rag_canarias/Services
```

---

## 🎓 CÓDIGO MÍNIMO PARA EMPEZAR

### Default.aspx
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="TuProyecto.Default" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Crawler</title>
</head>
<body>
    <form id="form1" runat="server">
        <h2>Web Crawler</h2>
        
        <div>
            <label>URL:</label>
            <asp:TextBox ID="txtUrl" runat="server" />
        </div>
        
        <div>
            <label>Max Páginas:</label>
            <asp:TextBox ID="txtMaxPages" runat="server" Text="50" />
        </div>
        
        <asp:Button ID="btnCrawl" runat="server" Text="Rastrear" OnClick="BtnCrawl_Click" />
        
        <div>
            <asp:Label ID="lblResult" runat="server" />
        </div>
    </form>
</body>
</html>
```

### Default.aspx.cs
```csharp
using System;
using System.IO;
using System.Web.UI;
using TuProyecto.Services;  // ← IMPORTANTE

public partial class Default : Page
{
    protected void BtnCrawl_Click(object sender, EventArgs e)
    {
        try
        {
            string url = txtUrl.Text;
            int maxPages = int.Parse(txtMaxPages.Text);
            
            // Resolver carpeta segura
            string appDataBase = Server.MapPath("~/App_Data/");
            string carpeta = PathHelper.ResolverRutaCarpeta(appDataBase, "");
            
            // Rastrear
            var crawler = new CrawlerService();
            var resultado = crawler.CrawlDominio(url, carpeta, maxPages);
            
            // Mostrar resultado
            if (resultado.Exitoso)
                lblResult.Text = $"✓ {resultado.PaginasDescargadas} páginas";
            else
                lblResult.Text = $"✗ {resultado.Mensaje}";
        }
        catch (Exception ex)
        {
            lblResult.Text = $"Error: {ex.Message}";
        }
    }
}
```

---

## 📊 CHECKLIST FINAL

```
[ ] Carpeta Services/ creada en Web Forms
[ ] CrawlerService.cs copiado
[ ] PathHelper.cs copiado
[ ] HtmlAgilityPack instalado (NuGet)
[ ] using [TuNamespace].Services; agregado
[ ] Formulario .aspx creado
[ ] Code-behind implementado
[ ] Carpeta App_Data/ creada
[ ] Compilación sin errores (Ctrl+Shift+B)
[ ] Prueba F5 - ¡debe funcionar!
```

---

## 🚨 ERRORES COMUNES

### Error: "The type or namespace name 'CrawlerService' could not be found"
**Solución:**
- Verifica que los archivos están en `Services/`
- Verifica el namespace en la línea 1 del archivo
- Agrega: `using [TuNamespace].Services;`

### Error: "HtmlAgilityPack is not installed"
**Solución:**
- Abre Package Manager Console
- `Install-Package HtmlAgilityPack`

### Error: "Cannot access App_Data"
**Solución:**
- Crea la carpeta manualmente: `App_Data/`
- Da permisos al usuario de IIS
- Ejecuta VS como Administrador

---

## ✨ LISTO PARA USAR

**Tu código MVC es 100% reutilizable.**

No necesitas cambiar NADA de CrawlerService.cs - es agnóstico.

Funciona igual en:
- ✅ ASP.NET Web Forms
- ✅ ASP.NET MVC
- ✅ Console Apps
- ✅ Windows Forms
- ✅ Cualquier .NET 4.6.1+

---

## 🔗 MÁS INFO

Documentación completa en:
- 📖 `GUIA_WEBFORMS.md` - Ejemplos detallados
- 📖 `PASOS_PARA_WEBFORMS.md` - Paso a paso visual
- 📖 `EXTRACCION_COMPONENTES.md` - Detalles técnicos
- 📖 `RESUMEN_EXPORTACION.md` - Resumen ejecutivo

---

**¡Listo! Copia los archivos y empieza a usar. 🚀**
