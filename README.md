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
- [Consideraciones Importantes](#-consideraciones-importantes)
- [Mejoras Futuras](#-mejoras-futuras)
- [Preguntas Frecuentes](#-preguntas-frecuentes)
- [Contacto](#-contacto)

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

---

## 🚀 Inicio Rápido

### Requisitos Previos
- **Visual Studio 2019+** (Community es suficiente)
- **.NET Framework 4.8.1** (incluido en VS 2019+)
- **IIS Express** (incluido en VS)
- **Acceso de escritura** al proyecto (para crear `App_Data/`)

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
   - Dejar URL vacía para usar seeds por defecto
   - Dejar "Carpeta de guardado" vacío para usar `App_Data/crawlings`
   - Ajustar `maxPages` (defecto 50) y `maxDepth` (defecto 2)
   - Pulsar "Iniciar crawling"
   - Revisar resultados en `App_Data\crawlings\<dominio>\`

---

## ⚙️ Uso y Configuración

### Parámetros del Formulario

| Parámetro | Tipo | Rango | Defecto | Descripción |
|-----------|------|-------|---------|------------|
| `url` | text | N/A | vacío | URL a rastrear. Si está vacía, se usan seeds por defecto. |
| `carpetaGuardado` | text | N/A | `App_Data/crawlings` | Ruta donde guardar los ficheros. Vacío = carpeta por defecto (dentro proyecto). Relativa (ej: `MisCrawls/enero`) o absoluta. |
| `maxPages` | int | 1–10000 | 50 | Máximo número de páginas a descargar del dominio. |
| `maxDepth` | int | 0–10 | 2 | Profundidad máxima de enlaces a seguir desde la página inicial. |
| `fullCrawl` | bool | true/false | false | Si se marca, permite hasta 1000 páginas (sin límite normal). |

### Cambios de Configuración Comunes

#### ✏️ Cambiar la carpeta de guardado por defecto

**Opción A: Desde la UI** (recomendado)
- En el formulario de entrada, rellena el campo "📁 Carpeta de guardado"
- Déjalo vacío para usar la carpeta por defecto: `App_Data/crawlings/`
- O introduce una ruta personalizada:
  - Relativa (ej: `MisCrawls/enero2024`) → se resuelve dentro del proyecto
  - Absoluta (ej: `D:\misCrawls\`) → se usa tal cual

**Opción B: Cambiar por defecto en código**

En `Controllers/HomeController.cs`, método `ResolverRutaCarpeta()`, línea ~225:
```csharp
// Antes:
return Server.MapPath("~/App_Data/crawlings/");

// Después (ejemplo: guardar en raíz del proyecto):
return Server.MapPath("~/crawlings/");
```

#### ✏️ Cambiar timeout de petición HTTP

En `CrawlDomain()`, línea ~85:
```csharp
// Antes:
client.Timeout = TimeSpan.FromSeconds(15);

// Después (más tolerante):
client.Timeout = TimeSpan.FromSeconds(30);
```

#### ✏️ Cambiar delay politeness entre peticiones

En `CrawlDomain()`, línea ~115:
```csharp
// Antes:
System.Threading.Thread.Sleep(300); // 300ms

// Después (más rápido, pero menos amigable):
System.Threading.Thread.Sleep(100); // 100ms
```

#### ✏️ Cambiar límite de `fullCrawl`

En `Crawl()`, línea ~40:
```csharp
// Antes:
int maxPaginasPorSitio = fullCrawl ? 1000 : Math.Max(1, maxPages);

// Después (permitir más):
int maxPaginasPorSitio = fullCrawl ? 5000 : Math.Max(1, maxPages);
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

---

## ❓ Preguntas Frecuentes

### P: ¿Por qué me sale error "No se puede acceder a la ruta"?
**R:** Verifica que:
1. La carpeta `App_Data/` existe en la raíz del proyecto (se crea automáticamente si no existe)
2. Tu usuario tiene permisos de escritura en el proyecto
3. Si indicaste una ruta personalizada, verifica que sea válida y que tengas permisos de escritura

**Solución rápida:** 
- Deja el campo "Carpeta de guardado" vacío para usar la carpeta por defecto
- O crea manualmente la carpeta `App_Data\crawlings\` en la raíz del proyecto

---

### P: ¿El crawler respeta robots.txt?
**R:** No, actualmente no lo implementa. Es una mejora futura. Asegúrate de verificar que tienes permiso para rastrear el dominio antes de ejecutar.

---

### P: ¿Puedo rastrear múltiples dominios a la vez?
**R:** No en la versión actual. El formulario acepta una URL, y se rastrea ese dominio. Para múltiples dominios, necesitarías enviar múltiples peticiones POST.

---

### P: ¿Por qué tarda tanto el crawling?
**R:** Hay un delay de **300ms** entre cada petición (politeness). Para 50 páginas, son ~15 segundos mínimo (sin contar descargas y procesamiento). Esto es intencional para no sobrecargar servidores.

**Reduce el delay** editando `System.Threading.Thread.Sleep(300)` en `CrawlDomain()` si el servidor lo permite.

---

### P: ¿Se guarda la contraseña si está en la URL?
**R:** ⚠️ **Sí, se guarda en los ficheros .txt**. Evita rastrear URLs que contengan credenciales (`https://user:pass@example.com`). Este es un riesgo de seguridad importante.

---

### P: ¿Funcionan las rutas personalizada en diferentes sistemas?
**R:** 
- **Windows**: Cualquier ruta es válida (`D:\MisCrawls\` o relativa dentro proyecto)
- **Linux/Mac**: Requiere rutas POSIX (`/home/user/MisCrawls/`). Relativas siempre funcionan.
- **Recomendado**: Usa rutas relativas (`MisCrawls/enero`) para máxima compatibilidad.

---

### P: ¿Puedo usar esto en producción?
**R:** **No recomendado sin cambios significativos**:
- ❌ Crawling síncrono bloqueará la aplicación
- ❌ Sin logging ni monitoreo
- ❌ Sin rate limiting global
- ❌ Sin respeto a robots.txt

**Recomendaciones para producción:**
1. Implementar background jobs (Hangfire, Azure Queue)
2. Añadir logging estructurado (Serilog)
3. Implementar robots.txt compliance
4. Usar base de datos para persistencia
5. Añadir autenticación y autorización
6. Implementar API REST versioned

---

## 📞 Contacto

- **Autor:** Luis Guillén
- **Repositorio:** [github.com/luis-guillen/rag_canarias_win](https://github.com/luis-guillen/rag_canarias_win)
- **Email:** Consulta en el repositorio
- **Estado:** Proyecto académico (TFG) — Completo para uso educativo y pruebas locales

---

## 📄 Licencia

Este proyecto está bajo licencia **MIT**. Consulta `LICENSE` para más detalles.

---

**Última actualización:** 2026 | **Versión:** 1.0 | **Status:** ✅ Completo
