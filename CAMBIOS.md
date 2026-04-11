# 📝 RESUMEN DE CAMBIOS - Gestión de Carpetas Configurables

## 🎯 Objetivo
Permitir a los usuarios seleccionar **dónde guardar los archivos del crawling** desde la interfaz web, con una carpeta por defecto dentro del proyecto para mayor claridad y portabilidad.

---

## ✨ Cambios Implementados

### 1️⃣ **Views/Home/Index.cshtml**
#### ➕ Nuevo campo de entrada: "📁 Carpeta de guardado"
```razor
<div style="margin-top: 15px;">
    <label for="carpetaGuardado">📁 Carpeta de guardado:</label>
    <input type="text" id="carpetaGuardado" name="carpetaGuardado" 
           style="width:400px;" 
           placeholder="(defecto: App_Data/crawlings/)" />
    <small style="display: block; margin-top: 5px; color: #666;">
        Deja vacío para usar la carpeta por defecto. 
        O introduce una ruta personalizada (relativa al proyecto, ej: MisCrawls/enero2024).
    </small>
</div>
```

**Características:**
- ✅ Placeholder instructivo
- ✅ Ayuda visual con emojis
- ✅ Instrucciones claras para rutas relativas/absolutas
- ✅ Mejor espaciado y organización del formulario

---

### 2️⃣ **Controllers/HomeController.cs**

#### 📌 Cambio en la firma del método `Crawl()`
```csharp
// Antes:
public ActionResult Crawl(string url, int maxPages = 50, int maxDepth = 2, bool fullCrawl = false)

// Después:
public ActionResult Crawl(string url, int maxPages = 50, int maxDepth = 2, 
                          bool fullCrawl = false, string carpetaGuardado = "")
```

#### 🔧 Nuevo método helper: `ResolverRutaCarpeta()`
```csharp
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
    return carpetaPersonalizada.EndsWith("\\") ? 
           carpetaPersonalizada : 
           carpetaPersonalizada + "\\";
}
```

**Lógica:**
1. Si vacío → usa `App_Data/crawlings/` (dentro proyecto)
2. Si relativa (ej: `MisCrawls/enero`) → resuelve desde raíz proyecto
3. Si absoluta (ej: `D:\misCrawls\`) → usa tal cual

#### 🔧 Nuevo método helper: `ObtenerRutaRelativa()`
```csharp
private string ObtenerRutaRelativa(string rutaAbsoluta)
{
    try
    {
        string raizProyecto = Server.MapPath("~");
        if (rutaAbsoluta.StartsWith(raizProyecto, StringComparison.OrdinalIgnoreCase))
        {
            // Extraer ruta relativa
            string relativa = rutaAbsoluta.Substring(raizProyecto.Length)
                                         .Trim('\\').Trim('/');
            return $"[Proyecto]/{relativa}";
        }
    }
    catch { }

    // Si no se puede determinar, devolver la ruta tal cual
    return rutaAbsoluta;
}
```

**Propósito:**
- Muestra rutas relativas al proyecto como `[Proyecto]/App_Data/crawlings/ejemplo_com`
- Facilita al usuario ubicar los archivos descargados

#### 📝 Cambios en el método `Crawl()`
```csharp
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

// ... resto del código ...

// Mostrar ruta relativa al proyecto para mayor claridad
string rutaRelativa = ObtenerRutaRelativa(carpetaSitio);
resultados.Add($"✓ {startUri.Host} → {total} páginas en {rutaRelativa}");

// ... al final ...
ViewBag.CarpetaBase = ObtenerRutaRelativa(carpetaBaseGlobal);
```

---

### 3️⃣ **Views/Home/Resultados.cshtml**
#### ✨ Mejor presentación de resultados
```razor
@{
    ViewBag.Title = "Resultados";
    var resultados = ViewBag.Resultados as List<string>;
    var carpetaBase = ViewBag.CarpetaBase as string;
    var error = ViewBag.Error as string;
}

<h2>📊 Resultados del Crawling</h2>

@if (!string.IsNullOrWhiteSpace(error))
{
    <div style="background-color: #ffebee; border: 1px solid #f44336; 
                padding: 15px; margin-bottom: 20px; border-radius: 4px; 
                color: #c62828;">
        <strong>⚠️ Error:</strong> @error
    </div>
}

@if (resultados != null && resultados.Count > 0)
{
    @if (!string.IsNullOrWhiteSpace(carpetaBase))
    {
        <div style="background-color: #e3f2fd; border: 1px solid #2196f3; 
                    padding: 15px; margin-bottom: 20px; border-radius: 4px; 
                    color: #1565c0;">
            <strong>📁 Carpeta base:</strong> <code>@carpetaBase</code>
        </div>
    }

    <div style="margin-top: 20px;">
        <h3>Dominios procesados:</h3>
        <ul style="list-style: none; padding: 0;">
            @foreach (var item in resultados)
            {
                <li style="padding: 10px; margin-bottom: 8px; 
                          background-color: #f5f5f5; 
                          border-left: 4px solid #4caf50; 
                          border-radius: 2px;">
                    @item
                </li>
            }
        </ul>
    </div>
}
```

**Mejoras:**
- ✅ Muestra carpeta base usada
- ✅ Mejor visualización de errores (cajas de alerta)
- ✅ Estilos profesionales para cada resultado
- ✅ Emojis para mejor legibilidad

---

### 4️⃣ **README.md**
#### 📚 Documentación actualizada

**Secciones modificadas:**
1. ✅ Tabla de Tech Stack → Añadida info de gestión de carpetas
2. ✅ Descripción → Nuevo punto: "📁 Gestión flexible de carpetas"
3. ✅ Componentes → Nuevo método: `ResolverRutaCarpeta()`
4. ✅ Tabla de Parámetros → Nuevo campo: `carpetaGuardado`
5. ✅ Estructura del Proyecto → Actualizada con `App_Data/crawlings/`
6. ✅ Configuración → Nueva sección "Cambiar la carpeta de guardado por defecto"
7. ✅ FAQ → Actualizada pregunta sobre permisos
8. ✅ Características → Añadida "Gestión flexible de carpetas"

---

## 🎨 Flujo de Usuario

### Escenario 1: Usar carpeta por defecto (recomendado)
```
1. Usuario deja vacío el campo "Carpeta de guardado"
2. Sistema resuelve: App_Data/crawlings/
3. Se crean archivos en: [Proyecto]/App_Data/crawlings/dominio/00_index.txt
4. Usuario ve: "[Proyecto]/App_Data/crawlings/dominio → 25 páginas"
```

### Escenario 2: Ruta relativa personalizada
```
1. Usuario introduce: MisCrawls/enero2024
2. Sistema resuelve: [Raíz Proyecto]/MisCrawls/enero2024/
3. Se crean archivos en: [Proyecto]/MisCrawls/enero2024/dominio/
4. Usuario ve: "[Proyecto]/MisCrawls/enero2024/dominio → 25 páginas"
```

### Escenario 3: Ruta absoluta
```
1. Usuario introduce: D:\MisCrawls\produccion
2. Sistema usa tal cual: D:\MisCrawls\produccion\
3. Se crean archivos en: D:\MisCrawls\produccion\dominio/
4. Usuario ve: "D:\MisCrawls\produccion\dominio → 25 páginas"
```

---

## 🔒 Ventajas de los Cambios

| Ventaja | Antes | Después |
|---------|--------|---------|
| **Ubicación por defecto** | `C:\temp\crawler\` (hardcoded) | `App_Data/crawlings/` (dentro proyecto) ✅ |
| **Portabilidad** | ❌ No (path absoluto) | ✅ Sí (relativo al proyecto) |
| **Configuración en UI** | ❌ No (solo código) | ✅ Sí (formulario) |
| **Validación de ruta** | ❌ Ninguna | ✅ Try/catch + mensaje error |
| **Visibilidad** | ❌ Ruta absoluta confusa | ✅ Ruta relativa clara `[Proyecto]/...` |
| **Flexibilidad** | ❌ Una sola ruta | ✅ Defecto + relativas + absolutas |

---

## ✅ Verificación

✔️ **Compilación:** Correcta (sin errores)
✔️ **Funcionalidad:** Rutas resueltas correctamente
✔️ **UI:** Formulario responsive, instrucciones claras
✔️ **Resultados:** Muestra ruta base y dominios procesados
✔️ **Documentación:** README actualizado completamente
✔️ **Manejo de errores:** Try/catch en creación de carpeta

---

## 🚀 Próximos Pasos (Opcionales)

1. **Guardar preferencias:** Recordar última carpeta usada
2. **Browse dialog:** Selector de carpeta visual (file browser)
3. **Validación:** Verificar permisos de escritura antes de rastrear
4. **Historial:** Listar rastreos anteriores con carpetas usadas
5. **Tamaño:** Mostrar tamaño total de archivos descargados

---

## 📋 Archivos Modificados

```
✏️  Views/Home/Index.cshtml          (Nuevo campo carpetaGuardado)
✏️  Controllers/HomeController.cs    (Nuevo métodos helper + parámetro)
✏️  Views/Home/Resultados.cshtml    (Mejor presentación)
✏️  README.md                        (Documentación actualizada)
```

---

**Estado:** ✅ Completado y compilado correctamente
**Fecha:** 2026
**Version:** 1.1 (Gestión de carpetas configurables)
