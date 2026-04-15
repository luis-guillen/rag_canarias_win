# 🎯 CHECKLIST: Qué copiar a tu proyecto Web Forms

## 📁 Estructura actual en el repo MVC

```
rag_canarias/
├── Controllers/
│   └── HomeController.cs                    ❌ NO COPIAR (es MVC)
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml                     ❌ NO COPIAR (es Razor)
│   │   └── Resultados.cshtml                ❌ NO COPIAR (es Razor)
│   └── Shared/
│       └── _Layout.cshtml                   ❌ NO COPIAR (es master MVC)
├── Content/
│   └── Site.css                             🤔 OPCIONAL (si quieres estilos)
├── Services/                                 ✅ COPIAR ESTA CARPETA COMPLETA
│   ├── CrawlerService.cs                    ✅✅✅ CRITICO
│   └── PathHelper.cs                        ✅✅✅ CRITICO
├── App_Start/                               ❌ NO COPIAR (config MVC)
├── GUIA_WEBFORMS.md                         📖 LEE ESTA
├── EXTRACCION_COMPONENTES.md                📖 LEE ESTA
└── packages.config                          🔍 VERIFICA (NuGet deps)
```

## ✅ Qué hacer PASO A PASO

### Paso 1: Copiar archivos
```
ORIGEN (este repo MVC):
  C:\Users\Luis\source\repos\rag_canarias\rag_canarias\Services\

DESTINO (tu repo Web Forms):
  [TU_CARPETA_WEBFORMS]\[TU_PROYECTO]\Services\
```

**Archivos a copiar exactamente:**
```
✅ CrawlerService.cs
✅ PathHelper.cs
```

### Paso 2: Verificar NuGet
Asegúrate de tener en tu `packages.config`:
```xml
<package id="HtmlAgilityPack" version="1.11.46" targetFramework="net481" />
```

Si no lo tienes:
```
Tools → NuGet Package Manager → Manage NuGet Packages
Buscar: HtmlAgilityPack
Instalar versión 1.11.46 o superior
```

### Paso 3: Crear la estructura Web Forms
En tu proyecto Web Forms, necesitas:
```
TuProyectoWebForms/
├── App_Data/                    ← Crear si no existe
├── Services/                    ← Crear si no existe
│   ├── CrawlerService.cs        ← PEGAR AQUI
│   └── PathHelper.cs            ← PEGAR AQUI
└── Default.aspx                 ← Crear tu formulario
```

### Paso 4: Crear Default.aspx.cs
Ver ejemplos en **GUIA_WEBFORMS.md** (section "Ejemplo en Default.aspx.cs")

### Paso 5: Probar
Ejecutar F5 en Visual Studio - debe compilar sin errores

## 🔍 Verificación rápida

Después de copiar, abre `CrawlerService.cs` en tu proyecto Web Forms y verifica:

```csharp
✅ Line 1: using HtmlAgilityPack;
✅ Line 6: namespace TU_NAMESPACE.Services
✅ Line 10: public class CrawlerService
✅ Línea con ResultadoCrawl debe estar definida
✅ Método CrawlDominio() debe existir
```

Si todo se ve igual, ¡está bien copiado!

## 🎯 Lo más importante

**SOLO NECESITAS 2 ARCHIVOS:**
1. `CrawlerService.cs` - El motor del crawler
2. `PathHelper.cs` - Validador de rutas

**TODO LO DEMÁS es específico de MVC y NO lo copies:**
- Controllers/ - ❌
- Views/ - ❌
- App_Start/ - ❌

## 💾 Resumen de lo que harás

```
ANTES:                          DESPUÉS:
Tu WebForms repo               Tu WebForms repo
├── Default.aspx              ├── Default.aspx
├── Default.aspx.cs           ├── Default.aspx.cs
├── Web.config                ├── Web.config
└── packages.config           ├── packages.config (+ HtmlAgilityPack)
                              ├── App_Data/
                              └── Services/
                                  ├── CrawlerService.cs  ← NUEVO
                                  └── PathHelper.cs      ← NUEVO
```

## 🚀 Una vez copiado

1. Lee `GUIA_WEBFORMS.md` para ver ejemplos completos
2. Copia el código del formulario ASPX
3. Copia el código del code-behind
4. Reemplaza valores de ejemplo con tus valores reales
5. Presiona F5 - ¡listo!

## ❗ Errores comunes al copiar

**"Error: The type or namespace name 'CrawlerService' could not be found"**
→ Verifica que:
  - Copiaste los archivos en la carpeta `Services/`
  - El namespace en el top del archivo es correcto
  - Agregaste `using TuNamespace.Services;`

**"Error: HtmlAgilityPack not found"**
→ Instala via NuGet: `Install-Package HtmlAgilityPack`

**"Error: Cannot resolve symbol 'CrawlDominio'"**
→ Asegúrate de que CrawlerService.cs está completo (no corrupto durante copia)

## 📞 Soporte

Si algo no funciona:
1. Verifica que los 2 archivos están en Services/
2. Verifica que HtmlAgilityPack está en packages.config
3. Limpia y reconstruye la solución (Build → Clean Solution, Build Solution)
4. Revisa GUIA_WEBFORMS.md para ejemplos
