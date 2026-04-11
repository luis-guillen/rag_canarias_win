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
- 📁 **Gestión flexible de carpetas**: guarda por defecto en proyecto, o en ruta personalizada

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
                                [ResolverRutaCarpeta() - ruta]
                                             ↓
                                    [CrawlDomain() - BFS Loop]
                                             ↓
                          [Descarga página + ExtraerTextoLimpio()]
                                             ↓
                      [GuardaFichero.txt en ruta configurada]
                                             ↓
                                    [Resultados.cshtml]
```

### Componentes Principales

#### 1. **Controlador: `HomeController.cs`**
- **`Crawl()`** — Punto de entrada (HttpPost)
  - Valida parámetros (`url`, `maxPages`, `maxDepth`, `fullCrawl`, `carpetaGuardado`)
  - Soporta múltiples seeds por defecto
  - Resuelve ruta de guardado (defecto o personalizada)
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

- **`ResolverRutaCarpeta()`** — Gestor de rutas
  - Defecto: `App_Data/crawlings` (dentro proyecto)
  - Soporta rutas relativas: resuelve desde raíz del proyecto
  - Soporta rutas absolutas: usa tal cual

#### 2. **Vistas:**

- **`Index.cshtml`** — Formulario de entrada
  - URL (text, opcional — si vacía usa seeds por defecto)
  - `carpetaGuardado` (text, opcional — deja vacío para usar `App_Data/crawlings`)
  - `maxPages` (number, rango: 1–10000, defecto: 50)
  - `maxDepth` (number, rango: 0–10, defecto: 2)
  - `fullCrawl` (checkbox, permite hasta 1000 páginas)
  - Botón "Iniciar crawling" (submit POST)

- **`Resultados.cshtml`** — Resumen de ejecución
  - Muestra carpeta base y lista de dominios procesados
  - Formato: `✓ ejemplo.com → 47 páginas en [Proyecto]/App_Data/crawlings/ejemplo_com`

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


## ✨ Características Principales

### 🔄 Crawling Inteligente
- ✅ Algoritmo BFS con control de profundidad
- ✅ Restricción automática a dominio único
- ✅ Filtro de URL binarias (`.exe`, `.zip`, etc.)
- ✅ Delay configurables entre peticiones (politeness)
- ✅ Detección y evitar loops infinitos
- ✅ Gestión flexible de carpetas (defecto o personalizada)

### 📄 Limpieza de Contenido
- ✅ Eliminación automática de `<script>`, `<style>`, `<noscript>`
- ✅ Decodificación de entidades HTML
- ✅ Normalización de espacios y saltos de línea
- ✅ Guardado en ficheros `.txt` puros (sin formato)

### 🖥️ Interfaz Intuitiva
- ✅ Formulario web Bootstrap 5
- ✅ Selector de carpeta de guardado
- ✅ Validación cliente y servidor
- ✅ Resumen visual de resultados
- ✅ Rutas relativas mostradas claramente

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
├── App_Data/                       # (se crea automáticamente)
│   └── crawlings/                  # Carpeta por defecto de guardado
│       └── ejemplo_com/            # Subcarpeta por dominio
│           ├── 00_index.txt
│           ├── 01_about.txt
│           └── ...
├── App_Start/
│   └── BundleConfig.cs             # Bundling de CSS/JS (bootstrap → site.css)
├── Web.config                      # Configuración ASP.NET
├── packages.config                 # Dependencias NuGet
├── rag_canarias.csproj             # Proyecto C#
└── README.md                       # Este archivo
```

---

## ⚠️ Consideraciones Importantes

### 🚨 Limitaciones Actuales

1. **Ejecución Síncrona**
   - El crawler se ejecuta dentro de la petición HTTP
   - Para sitios grandes (>100 páginas), puede causar **timeout en IIS** (120s default)
   - La UI permanece bloqueada hasta finalizar
   - ❌ No es adecuado para producción sin optimizaciones

2. **Falta de Logging**
   - No hay registro de errores o estadísticas de rastreo
   - Difícil depurar si algo falla silenciosamente

3. **Sin Respeto a robots.txt**
   - El crawler no verifica `robots.txt` del dominio
   - Puede ser considerado impolite o ilegal en ciertos contextos

4. **Permisos de Archivo**
   - Requiere permisos de escritura en el proyecto
   - Si ejecuta bajo cuenta restringida (ej: IUSR), puede fallar

5. **Manejo de Errores Básico**
   - Errores en descargas pueden silenciarse
   - Sin retry automático

### ✅ Buenas Prácticas Implementadas

- ✅ Delay de 300ms entre peticiones (politeness)
- ✅ Timeout de 15s en HttpClient (evitar cuelgues)
- ✅ Validación de URL (solo `http://https://`)
- ✅ Restricción de dominio (evitar crawling cruzado)
- ✅ Límite de profundidad (evitar rastreo infinito)
- ✅ Límite de `fullCrawl` a 1000 páginas (seguridad)
- ✅ Rutas configurables desde UI (flexibilidad)

### 🔒 Consideraciones de Seguridad

- ⚠️ **Validar entrada de URL** — Usar whitelist de dominios si es crítico
- ⚠️ **Rate limiting** — Implementar límite global de rastreos concurrentes
- ⚠️ **Permisos de carpeta** — Ejecutar bajo cuenta con mínimos permisos necesarios
- ⚠️ **Términos de servicio** — Verificar que el dominio permite crawling automatizado

---

## 🚀 Mejoras Futuras

### 🔜 Prioridad Alta
- [ ] **Background jobs** — Usar Hangfire o Azure Queue para crawling asincrónico
- [ ] **Logging estructurado** — Serilog con niveles (Info, Warning, Error)
- [ ] **Respeto a robots.txt** — Descargar y parsear antes de rastrear
- [ ] **Progress real-time** — SignalR para actualizar UI durante crawling
- [ ] **Caché de dominios** — No re-crawlear el mismo dominio en corto plazo

### 🔜 Prioridad Media
- [ ] **Descarga de ficheros** — UI para descargar `.zip` de resultados
- [ ] **Estadísticas** — Tabla de dominios rastreados, horas, páginas/minuto
- [ ] **Whitelist/Blacklist** — Permitir/denegar dominios específicos
- [ ] **API REST** — Endpoints para integración con otros servicios
- [ ] **Batch crawling** — Subir lista de URLs para rastrear masivamente

### 🔜 Prioridad Baja
- [ ] **Migración a .NET Core/.NET 8** — Modernización del stack
- [ ] **Docker support** — Containerizar para deployment
- [ ] **Autenticación** — Login para aislar rastreos por usuario
- [ ] **Base de datos** — SQLite/SQL Server para persistencia de rastreos
- [ ] **OCR opcional** — Extraer texto de imágenes embebidas


## 📄 Licencia

Este proyecto está bajo licencia **MIT**. Consulta `LICENSE` para más detalles.

---

**Última actualización:** 2026 | **Versión:** 1.0 | **Status:** ✅ Completo
