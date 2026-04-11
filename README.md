# 🕷️ RAG Canarias

> **Proyecto ASP.NET MVC para crawling web y limpieza de contenido HTML**  
> Trabajo de Fin de Grado — Aplicación para descargar y procesar páginas web de un dominio de forma automática.

[![.NET Framework](https://img.shields.io/badge/.NET-Framework%204.8.1-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-7.3-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![ASP.NET MVC](https://img.shields.io/badge/ASP.NET-MVC%205.2-0078D4?logo=microsoft)](https://dotnet.microsoft.com/apps/aspnet)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.2.3-7952B3?logo=bootstrap)](https://getbootstrap.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📋 Tabla de Contenidos

- [Descripción](#-descripción)
- [Stack Tecnológico](#-stack-tecnológico)
- [Arquitectura](#-arquitectura)
- [Inicio Rápido](#-inicio-rápido)
- [Uso y Configuración](#-uso-y-configuración)
- [Características Principales](#-características-principales)
- [Estructura del Proyecto](#-estructura-del-proyecto)
---

## 📖 Descripción

**RAG Canarias** es una aplicación web que implementa un **crawler web con control de profundidad** para descargar y procesar automáticamente todas las páginas de un dominio. 

**Características clave:**
- 🔄 Algoritmo **BFS (Breadth-First Search)** para rastreo eficiente
- 🧹 **Limpieza HTML automática**: elimina scripts, estilos y etiquetas innecesarias
- 📄 **Guardado por página**: cada URL se descarga en un fichero `.txt` separado con solo texto limpio
- 🌐 **Restricción de dominio**: respeta automáticamente los límites del dominio
- 🕐 **Control de profundidad**: limita el número de niveles de navegación
- 📊 **Interfaz web intuitiva**: formulario para configurar parámetros de rastreo
- 🌙 **Tema oscuro integrado**: con Font Awesome y persistencia en localStorage

---

## 🛠️ Stack Tecnológico

| Categoría | Tecnología | Versión | Propósito |
|-----------|-----------|---------|----------|
| **Lenguaje** | C# | 7.3 | Código backend y lógica de aplicación |
| **Runtime** | .NET Framework | 4.8.1 | Plataforma de ejecución |
| **Web Framework** | ASP.NET MVC | 5.2.x | Controladores, vistas y enrutamiento |
| **Template Engine** | Razor | - | Vistas dinámicas (.cshtml) |
| **HTML Parsing** | HtmlAgilityPack | Latest (NuGet) | DOM parsing y XPath queries |
| **HTTP Client** | System.Net.Http | - | Peticiones HTTP (built-in .NET) |
| **CSS Framework** | Bootstrap | 5.2.3 | Componentes UI y responsive design |
| **Iconos** | Font Awesome | 6.4.0 (CDN) | Iconos del toggle de tema oscuro |
| **Servidor** | IIS Express | - | Desarrollo local |
| **Control de Versión** | Git | - | Repositorio en GitHub |

---

## 🏗️ Arquitectura

### Flujo de Ejecución

```
[Usuario] → [Formulario Index.cshtml] → [HomeController.Crawl()]
                                             ↓
                                    [Validación de parámetros]
                                             ↓
                                    [CrawlDomain() - BFS Loop]
                                             ↓
                          [Descarga página + ExtractText()]
                                             ↓
                          [GuardaFichero.txt en C:\temp\crawler\]
                                             ↓
                                    [Resultados.cshtml]
```

### Componentes Principales

#### 1. **Controlador: `HomeController.cs`**
- **`Crawl()`** — Punto de entrada (HttpPost)
  - Valida parámetros (`url`, `maxPages`, `maxDepth`, `fullCrawl`)
  - Soporta múltiples seeds por defecto
  - Aplica límite seguro: `fullCrawl` máximo **1000 páginas**

- **`CrawlDomain()`** — Motor BFS
  - `Queue<Tuple<Uri, int>>` con profundidad integrada
  - Respeta `maxDepth`: no expande enlaces cuando `depth >= maxDepth`
  - HttpClient con timeout de **15 segundos**
  - Delay politeness de **300ms** entre peticiones
  - Detección y filtro de URLs no rastreables (extensiones binarias, etc.)

- **`ExtraerTextoLimpio()`** — Limpieza HTML
  - XPath: `//script | //style | //noscript` → eliminadas
  - Decodificación de entidades HTML (`DeEntitize()`)
  - Trim de líneas vacías y normalización de espacios

- **`ExtraerEnlacesInternos()`** — Extracción de URLs
  - XPath: `//a[@href]` para todos los links
  - Validación: solo `http://` y `https://`
  - Restricción de dominio: `Uri.Host` debe coincidir
  - Normalización de rutas relativas (`Uri.TryCreate()`)

#### 2. **Vistas:**

- **`Index.cshtml`** — Formulario de entrada
  - URL (text, opcional — si vacía usa seeds por defecto)
  - `maxPages` (number, rango: 1–10000, defecto: 50)
  - `maxDepth` (number, rango: 0–10, defecto: 2)
  - `fullCrawl` (checkbox, permite hasta 1000 páginas)
  - Botón "Iniciar crawling" (submit POST)

- **`Resultados.cshtml`** — Resumen de ejecución
  - Lista cada dominio rastreado con cantidad de páginas y ruta de guardado
  - Formato: `"ejemplo.com → 47 páginas guardadas en C:\temp\crawler\ejemplo_com\"`

#### 3. **Tema Oscuro (`_Layout.cshtml` + `Site.css`)**

- **Script en `<head>`** (ejecución pre-paint):
  ```javascript
  // Lee localStorage/cookie ANTES de primer render
  // Aplica clase 'dark-mode' a <html>
  // Evita flicker claro→oscuro
  ```

- **Diseño de tokens CSS** (`Site.css`):
  - Variables CSS custom: `--bg-color`, `--text-color`, `--input-bg`, etc.
  - `html.dark-mode { ... }` redefine todas las variables
  - Overrides Bootstrap con especificidad alta + `!important`
  - Transiciones suaves (0.15s) en colores

- **Toggle Button**:
  - Botón flotante en navbar
  - Icono Font Awesome: `fa-moon` (oscuro) ↔ `fa-sun` (claro)
  - Evento click → toggle clase `dark-mode` en `html`
  - Persistencia: `localStorage.setItem('darkMode', isDark)` + cookie fallback

---

## 🚀 Inicio Rápido

### Requisitos Previos
- **Visual Studio 2019+** (Community es suficiente)
- **.NET Framework 4.8.1** (incluido en VS 2019+)
- **IIS Express** (incluido en VS)
- **Acceso de escritura** en `C:\temp\` (o carpeta alternativa configurada)

### Pasos de Instalación

1. **Clonar el repositorio**
   ```powershell
   git clone https://github.com/luis-guillen/rag_canarias_win.git
   cd rag_canarias
   ```

2. **Abrir en Visual Studio**
   ```powershell
   # Desde el directorio del proyecto
   explorer rag_canarias.sln
   ```

3. **Restaurar dependencias NuGet**
   - Clic derecho en Solución → "Restore NuGet Packages"
   - O desde Package Manager Console:
     ```powershell
     Update-Package -Reinstall
     ```

4. **Ejecutar localmente**
   - Presionar **F5** (Debug) o **Ctrl+F5** (Sin debug)
   - Se abre automáticamente `https://localhost:<puerto>/Home/Index`

5. **Probar el crawler**
   - Dejar URL vacía para usar seeds por defecto (ej: `www.ejemplo.com`)
   - O introducir una URL válida (ej: `https://ejemplo.com`)
   - Ajustar `maxPages` (defecto 50) y `maxDepth` (defecto 2)
   - Pulsar "Iniciar crawling"
   - Revisar resultados y ficheros en `C:\temp\crawler\<dominio>\`

---

## ⚙️ Uso y Configuración

### Parámetros del Formulario

| Parámetro | Tipo | Rango | Defecto | Descripción |
|-----------|------|-------|---------|------------|
| `url` | text | N/A | vacío | URL a rastrear. Si está vacía, se usan seeds por defecto. |
| `maxPages` | int | 1–10000 | 50 | Máximo número de páginas a descargar del dominio. |
| `maxDepth` | int | 0–10 | 2 | Profundidad máxima de enlaces a seguir desde la página inicial. |
| `fullCrawl` | bool | true/false | false | Si se marca, permite hasta 1000 páginas (sin límite normal). |

### Cambios de Configuración Comunes

#### ✏️ Cambiar la carpeta de salida

En `Controllers/HomeController.cs`, línea ~45:
```csharp
// Antes:
string carpetaBaseGlobal = @"C:\temp\crawler\";

// Después (ejemplo):
string carpetaBaseGlobal = @"D:\misCrawls\";
```
✅ **Asegúrate** de que la cuenta que ejecuta IIS Express tiene permisos de escritura en esa ruta.

#### ✏️ Cambiar timeout de petición HTTP

En `CrawlDomain()`, línea ~XX:
```csharp
// Antes:
client.Timeout = TimeSpan.FromSeconds(15);

// Después (más tolerante):
client.Timeout = TimeSpan.FromSeconds(30);
```

#### ✏️ Cambiar delay politeness entre peticiones

En `CrawlDomain()`, línea ~YY:
```csharp
// Antes:
System.Threading.Thread.Sleep(300); // 300ms

// Después (más rápido, pero menos amigable):
System.Threading.Thread.Sleep(100); // 100ms
```

#### ✏️ Cambiar límite de `fullCrawl`

En `Crawl()`, línea ~ZZ:
```csharp
// Antes:
if (fullCrawl) maxPages = Math.Min(maxPages, 1000);

// Después (permitir más):
if (fullCrawl) maxPages = Math.Min(maxPages, 5000);
```

#### ✏️ Ajustar estilos oscuros

En `Content/Site.css`, buscar sección "Design tokens – dark":
```css
html.dark-mode {
    --bg-color: #121212;      /* Cambiar color de fondo */
    --text-color: #e0e0e0;    /* Cambiar color de texto */
    --navbar-bg: #1e1e1e;     /* Cambiar color navbar */
    /* ... más variables ... */
}
```

---

## ✨ Características Principales

### 🔄 Crawling Inteligente
- ✅ Algoritmo BFS con control de profundidad
- ✅ Restricción automática a dominio único
- ✅ Filtro de URL binarias (`.exe`, `.zip`, etc.)
- ✅ Delay configurables entre peticiones (politeness)
- ✅ Detección y evitar loops infinitos

### 📄 Limpieza de Contenido
- ✅ Eliminación automática de `<script>`, `<style>`, `<noscript>`
- ✅ Decodificación de entidades HTML
- ✅ Normalización de espacios y saltos de línea
- ✅ Guardado en ficheros `.txt` puros (sin formato)

### 🖥️ Interfaz Intuitiva
- ✅ Formulario web Bootstrap 5
- ✅ Validación cliente y servidor
- ✅ Resumen visual de resultados
- ✅ Indicación de progreso (parámetros aplicados)

### 🌙 Tema Oscuro
- ✅ Font Awesome 6.4.0 CDN
- ✅ Toggle persistente (localStorage + cookie)
- ✅ Sin parpadeos al cargar (script pre-paint)
- ✅ Overrides Bootstrap profesionales
- ✅ Transiciones suaves (0.15s)

---

## 📁 Estructura del Proyecto

```
rag_canarias/
├── Controllers/
│   └── HomeController.cs           # Motor del crawler (Crawl, CrawlDomain, etc.)
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml            # Formulario de entrada
│   │   └── Resultados.cshtml       # Página de resultados
│   └── Shared/
│       └── _Layout.cshtml          # Master layout, navbar, toggle oscuro
├── Content/
│   ├── bootstrap.css               # Bootstrap 5.2.3
│   └── Site.css                    # Estilos personalizados + dark-mode
├── App_Start/
│   └── BundleConfig.cs             # Bundling de CSS/JS (bootstrap → site.css)
├── Web.config                      # Configuración ASP.NET
├── packages.config                 # Dependencias NuGet
├── rag_canarias.csproj             # Proyecto C#
└── README.md                       # Este archivo

# Estructura de salida (generada durante crawling):
C:\temp\crawler\
└── ejemplo_com/                    # Carpeta por dominio
    ├── 00_index.txt
    ├── 01_about.txt
    ├── 02_products.txt
    └── ...
```

---


## 📄 Licencia

Este proyecto está bajo licencia **MIT**. Consulta `LICENSE` para más detalles.

---

**Última actualización:** 2026 | **Versión:** 1.0 | **Status:** ✅ Completo
