# 🚀 INSTRUCCIONES DE USO - Gestión de Carpetas Configurables

## 🎯 Resumen Rápido

Acabas de actualizar tu aplicación RAG Canarias con la capacidad de **seleccionar dónde guardar los archivos del crawling** desde la interfaz web.

**Lo más importante:** Ahora los archivos se guardan en **`App_Data/crawlings`** (dentro del proyecto) por defecto, no en `C:\temp\`.

---

## ⚡ Cómo Usar (Inicio Rápido)

### Paso 1: Ejecutar la aplicación
```powershell
# En Visual Studio: F5 o Ctrl+F5
# Se abrirá: http://localhost:XXXX/Home/Index
```

### Paso 2: Rellenar el formulario
```
URL: https://ejemplo.com
  (o dejar vacío para usar seeds por defecto)

📁 Carpeta de guardado: (dejar VACÍO)
  ↓
  → Usa automáticamente: App_Data/crawlings/

MaxPages: 50
MaxDepth: 2
FullCrawl: (sin marcar)
```

### Paso 3: Pulsar "Iniciar crawling"
```
La aplicación:
1. Resuelve la ruta → App_Data/crawlings/
2. Crea carpeta → App_Data\crawlings\ejemplo_com\
3. Descarga páginas
4. Limpia HTML y guarda en .txt
```

### Paso 4: Ver resultados
```
✓ ejemplo.com → 50 páginas en [Proyecto]/App_Data/crawlings/ejemplo_com

Archivos guardados en:
C:\Users\[TuUsuario]\source\repos\rag_canarias\App_Data\crawlings\
```

---

## 🎨 Opciones de Carpeta

### Opción 1: DEFECTO (recomendado)
```
Deja el campo VACÍO

↓ Sistema resuelve:
  [Proyecto]/App_Data/crawlings/

✨ Ventajas:
  ✓ No requiere configuración
  ✓ Automático y claro
  ✓ Dentro del proyecto
  ✓ Fácil de empaquetar
```

### Opción 2: RUTA RELATIVA
```
Introduce: MisCrawls/enero2024

↓ Sistema resuelve:
  [Proyecto]/MisCrawls/enero2024/

📝 Ejemplo:
  C:\...\rag_canarias\MisCrawls\enero2024\

✨ Ventajas:
  ✓ Organización personalizada
  ✓ Temporal por mes/año
  ✓ Múltiples rastreos
```

### Opción 3: RUTA ABSOLUTA
```
Introduce: D:\misCrawls\produccion

↓ Sistema usa tal cual:
  D:\misCrawls\produccion\

✨ Ventajas:
  ✓ Almacenamiento externo
  ✓ Mejor rendimiento
  ✓ Separado del código
```

---

## 📋 Ejemplos de Rutas Válidas

### ✅ CORRECTAS

```
Relativas (dentro proyecto):
  └─ Dejar vacío
  └─ MisCrawls
  └─ MisCrawls/enero
  └─ Investigacion/casos/caso_001
  └─ Backups/2024/enero

Absolutas (fuera proyecto):
  └─ D:\misCrawls
  └─ D:\Crawls\produccion\patrimonio
  └─ E:\BACKUP\webs
  └─ \\servidor\compartida\crawls
```

### ❌ INCORRECTAS

```
❌ ../../../etc/passwd (escape de proyecto)
❌ C:\Windows\System32 (sin permisos)
❌ CON:, PRN, LPT1 (nombres reservados Windows)
❌ /dev/null, /etc/shadow (Linux protegido)
```

---

## 🔧 Cambios Técnicos (Programadores)

### Nuevo Parámetro
```csharp
public ActionResult Crawl(
    string url, 
    int maxPages = 50, 
    int maxDepth = 2, 
    bool fullCrawl = false,
    string carpetaGuardado = ""  // ← NUEVO
)
```

### Nuevos Métodos
```csharp
// Resuelve ruta basada en input del usuario
private string ResolverRutaCarpeta(string carpetaPersonalizada)

// Convierte ruta absoluta a relativa para mostrar
private string ObtenerRutaRelativa(string rutaAbsoluta)
```

### Cambios en HomeController.cs
```csharp
// Línea ~40: Resolver ruta
string carpetaBaseGlobal = ResolverRutaCarpeta(carpetaGuardado);

// Línea ~50: Validar creación
try {
    Directory.CreateDirectory(carpetaBaseGlobal);
} catch (Exception ex) {
    ViewBag.Error = $"Error al crear carpeta: {ex.Message}";
    return View("Resultados");
}

// Línea ~80: Mostrar ruta relativa en resultados
string rutaRelativa = ObtenerRutaRelativa(carpetaSitio);
resultados.Add($"✓ {startUri.Host} → {total} páginas en {rutaRelativa}");
```

---

## 🧪 Pruebas Sugeridas

### Test 1: Ruta Defecto
```
1. Deja "Carpeta de guardado" vacío
2. Ejecuta rastreo
3. Verifica: App_Data\crawlings\[dominio]\
4. ✅ Resultado: Archivos en App_Data/crawlings
```

### Test 2: Ruta Relativa
```
1. Introduce: MisCrawls/test
2. Ejecuta rastreo
3. Verifica: MisCrawls\test\[dominio]\
4. ✅ Resultado: Archivos en MisCrawls/test
```

### Test 3: Ruta Absoluta
```
1. Introduce: D:\test_crawls
2. Ejecuta rastreo
3. Verifica: D:\test_crawls\[dominio]\
4. ✅ Resultado: Archivos en D:\test_crawls
```

### Test 4: Ruta Inválida
```
1. Introduce: ruta\sin\permisos
2. Ejecuta rastreo
3. Verifica: Mensaje de error claro
4. ✅ Resultado: Error mostrado, sin crash
```

---

## 📚 Documentos Disponibles

```
README.md
  └─ Documentación completa del proyecto (ACTUALIZADO)

CAMBIOS.md
  └─ Detalles técnicos de la implementación

RESUMEN_FINAL.md
  └─ Resumen ejecutivo de cambios

GUIA_VISUAL.md
  └─ Ejemplos visuales y diagramas ASCII
```

---

## ⚙️ Configuración Avanzada

### Cambiar Ruta Por Defecto en Código

En `Controllers/HomeController.cs`, método `ResolverRutaCarpeta()`:

```csharp
// Antes:
return Server.MapPath("~/App_Data/crawlings/");

// Después (ejemplo: carpeta raíz):
return Server.MapPath("~/crawlings/");
```

### Cambiar Timeout HTTP

En `CrawlDomain()`, línea ~85:
```csharp
client.Timeout = TimeSpan.FromSeconds(15);  // ← Cambiar a 30 o 60
```

### Cambiar Delay Entre Peticiones

En `CrawlDomain()`, línea ~115:
```csharp
System.Threading.Thread.Sleep(300);  // ← Cambiar a 100 o 500
```

---

## ✅ Checklist de Verificación

- ✅ Aplicación compila sin errores
- ✅ Formulario tiene campo "📁 Carpeta de guardado"
- ✅ Ruta defecto funciona (App_Data/crawlings)
- ✅ Rutas relativas funcionan
- ✅ Rutas absolutas funcionan
- ✅ Errores mostrados claramente
- ✅ Resultados muestran ruta base
- ✅ Archivos se guardan correctamente
- ✅ README actualizado
- ✅ Commits realizados en Git

---

## 🆘 Solución de Problemas

### Problema: "No se puede acceder a la ruta"
```
Soluciones:
1. Deja "Carpeta de guardado" vacío (usa defecto)
2. O crea carpeta manualmente: App_Data\crawlings\
3. Verifica permisos de escritura
```

### Problema: "Ruta no encontrada"
```
Soluciones:
1. Usa rutas relativas (MisCrawls/enero)
2. O rutas absolutas con permisos (D:\crawls)
3. Evita caracteres especiales
```

### Problema: "Archivos no se guardan"
```
Soluciones:
1. Verifica que carpeta existe
2. Verifica permisos de usuario
3. Mira console para mensajes de error
```

---

## 📞 Soporte

- **Documentación:** Ver README.md
- **Cambios técnicos:** Ver CAMBIOS.md
- **Ejemplos visuales:** Ver GUIA_VISUAL.md
- **Repositorio:** https://github.com/luis-guillen/rag_canarias_win

---

## 🎉 ¡Listo!

Tu aplicación RAG Canarias ahora tiene **gestión configurable de carpetas**.

**Próximos pasos:**
1. Prueba la aplicación localmente
2. Verifica que los archivos se guardan en App_Data/crawlings
3. Personaliza rutas si lo necesitas
4. ¡Disfruta el crawler mejorado!

---

**Versión:** 1.1  
**Estado:** ✅ Completo  
**Fecha:** 2026
