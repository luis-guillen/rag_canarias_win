# 📦 EXTRACCIÓN DE COMPONENTES PARA WEB FORMS

## ✅ Archivos listos para copiar

Tu proyecto MVC ya tiene extraído todo lo necesario en la carpeta `Services/`:

```
rag_canarias/Services/
├── CrawlerService.cs     (🎯 MAIN - Toda la lógica del crawler)
└── PathHelper.cs         (🎯 UTILITIES - Manejo de rutas portables)
```

## 📋 Qué necesitas de este repo para Web Forms

### 1. **Obligatorio - Copiar estos archivos:**

```
Services/CrawlerService.cs      → [TuProyectoWebForms]/Services/CrawlerService.cs
Services/PathHelper.cs          → [TuProyectoWebForms]/Services/PathHelper.cs
```

### 2. **Dependencias NuGet necesarias:**

```xml
<!-- packages.config -->
<package id="HtmlAgilityPack" version="1.11.46" targetFramework="net481" />
```

O instalar con Package Manager:
```powershell
Install-Package HtmlAgilityPack
```

### 3. **Estructura de carpetas:**

En tu proyecto Web Forms necesitas:
```
TuProyectoWebForms/
├── App_Data/                 ← CREAR si no existe (para guardar crawls)
├── Services/                 ← CREAR si no existe
│   ├── CrawlerService.cs     ← COPIAR AQUI
│   └── PathHelper.cs         ← COPIAR AQUI
├── Default.aspx              ← Tu formulario
├── Default.aspx.cs           ← Tu code-behind (ver GUIA_WEBFORMS.md)
└── Web.config                ← Asegúrate de tener AppSettings normales
```

### 4. **Cambios en namespaces:**

En tu `Default.aspx.cs`:

```csharp
using System;
using System.IO;
using System.Web.UI;
using TuNamespace.Services;  // ← CAMBIAR a tu namespace
```

## 🎯 Lo que NO necesitas copiar:

- ❌ `Controllers/HomeController.cs` - Esto es específico de MVC
- ❌ `Views/` - Completo, es específico de MVC/Razor
- ❌ `App_Start/` - Configuración MVC, no necesaria en Web Forms
- ❌ `Content/Site.css` - Opcional, puedes tener el tuyo
- ❌ `.gitignore` - Usa el de tu proyecto Web Forms

## 📊 Resumen de componentes

| Archivo | Propósito | Reutilizable | Copiar |
|---------|----------|-------------|--------|
| `CrawlerService.cs` | Motor del crawler + extracción de texto | ✅ SÍ | ✅ SIEMPRE |
| `PathHelper.cs` | Validación y manejo de rutas | ✅ SÍ | ✅ SIEMPRE |
| `HomeController.cs` | Orquestador de MVC | ❌ NO | ❌ NUNCA |
| `_Layout.cshtml` | Master page MVC | ❌ NO | ❌ NUNCA |
| `Index.cshtml` | Vista MVC | ❌ NO | ❌ NUNCA |
| `Resultados.cshtml` | Vista MVC | ❌ NO | ❌ NUNCA |
| `Site.css` | Estilos | ✅ OPCIONAL | 🤔 OPCIONAL |
| `packages.config` | Dependencias | ✅ VERIFICAR | 🔍 VERIFICAR |

## 🔗 Clase CrawlerService: API pública

```csharp
public class CrawlerService
{
    // MÉTODOS PÚBLICOS PRINCIPALES:
    
    public ResultadoCrawl CrawlDominio(
        string urlSemilla,
        string carpetaGuardado,
        int maxPaginas = 50,
        int maxDepth = 2
    )
    // → Realiza crawling completo y retorna ResultadoCrawl
    
    public string ExtraerTextoLimpio(string html)
    // → Limpia HTML y retorna solo texto útil
    
    public string GenerarNombreCarpetaDominio(Uri uri)
    // → Genera nombre de carpeta seguro para un dominio

    // CLASE ANIDADA - Resultado
    public class ResultadoCrawl
    {
        public bool Exitoso { get; set; }              // ¿Funcionó?
        public string Mensaje { get; set; }           // Descripción
        public int PaginasDescargadas { get; set; }   // Cuántas bajó
        public string RutaRelativa { get; set; }      // Dónde guardó
        public Exception Excepcion { get; set; }      // Error si hay
    }
}
```

## 🔗 Clase PathHelper: API pública

```csharp
public class PathHelper
{
    public static string ResolverRutaCarpeta(
        string appDataBase,              // Server.MapPath("~/App_Data/")
        string carpetaPersonalizada = "" // Nombre subcarpeta o vacío
    )
    // → Retorna ruta absoluta segura dentro de App_Data
    // → VALIDA: rechaza path traversal, rutas absolutas, etc.
    
    public static string ObtenerRutaRelativa(
        string raizProyecto,    // Server.MapPath("~")
        string rutaAbsoluta     // Ruta a convertir
    )
    // → Retorna "ruta relativa" con formato [Proyecto]/...
}
```

## 🚀 Pasos rápidos para integrar

1. **Copiar archivos:**
```powershell
# En PowerShell, desde tu PC
Copy-Item -Path "C:\Users\Luis\source\repos\rag_canarias\Services\*" `
          -Destination "D:\TuProyectoWebForms\Services\" -Force
```

2. **Instalar NuGet:**
```powershell
# En Package Manager Console de Visual Studio
Install-Package HtmlAgilityPack -Version 1.11.46
```

3. **Crear formulario ASPX** (ver GUIA_WEBFORMS.md)

4. **Listo** - Ya funciona el crawler en Web Forms

## 📚 Documentación completa

Ver archivo: **GUIA_WEBFORMS.md** para:
- Ejemplos completos de uso
- Código ASPX y code-behind
- Tips avanzados
- Troubleshooting

## ❓ Preguntas comunes

**P: ¿Necesito cambiar algo en CrawlerService.cs?**
R: No. Es agnóstico a MVC/Web Forms. Cópialo tal cual.

**P: ¿Qué pasa con App_Data?**
R: CrawlerService usa rutas absolutas. PathHelper valida que todo esté dentro de App_Data.

**P: ¿Puedo usar esto en otros frameworks?**
R: Sí. CrawlerService no tiene dependencias de ASP.NET. Funciona en Console Apps, Windows Forms, etc.

**P: ¿Necesito modificar Web.config?**
R: No. Solo asegúrate de que App_Data tiene permisos de escritura.

**P: ¿Puedo tener múltiples instancias crawling?**
R: Sí. Crea múltiples instancias de CrawlerService o usa Tasks para paralelizar.

---

**¡Listo! Tu código del crawler ahora es totalmente portátil y reutilizable.**
