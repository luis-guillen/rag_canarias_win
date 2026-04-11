# 🎨 GUÍA VISUAL - Gestión de Carpetas Configurables

## 📱 Interfaz de Usuario

### Formulario Principal (Index.cshtml)

```
╔════════════════════════════════════════════════════════════════╗
║  🕷️ Crawler Patrimonio Canarias                               ║
║  Procesar una URL o las webs semilla iniciales del TFG.        ║
╠════════════════════════════════════════════════════════════════╣
║                                                                ║
║  URL (opcional):                                              ║
║  ┌──────────────────────────────────────────────────────────┐ ║
║  │ https://ejemplo.com                                      │ ║
║  └──────────────────────────────────────────────────────────┘ ║
║  Si está vacía, se usan URLs de prueba por defecto.           ║
║                                                                ║
║  📁 Carpeta de guardado:                                       ║
║  ┌──────────────────────────────────────────────────────────┐ ║
║  │ (defecto: App_Data/crawlings/)                           │ ║
║  └──────────────────────────────────────────────────────────┘ ║
║  Deja vacío para usar la carpeta por defecto.                 ║
║  O introduce una ruta personalizada (relativa al proyecto,    ║
║  ej: MisCrawls/enero2024).                                    ║
║                                                                ║
║  MaxPages:                                                    ║
║  ┌──────────┐                                                ║
║  │ 50       │                                                ║
║  └──────────┘                                                ║
║                                                                ║
║  MaxDepth:                                                    ║
║  ┌──────────┐                                                ║
║  │ 2        │                                                ║
║  └──────────┘                                                ║
║                                                                ║
║  FullCrawl (permitir hasta 1000 páginas):                     ║
║  ☐ (checkbox sin marcar)                                      ║
║                                                                ║
║  ┌─────────────────────────────────┐                         ║
║  │ Iniciar crawling                │                         ║
║  └─────────────────────────────────┘                         ║
║                                                                ║
╚════════════════════════════════════════════════════════════════╝
```

---

## 📊 Página de Resultados

### Con Carpeta Base Mostrada

```
╔════════════════════════════════════════════════════════════════╗
║  📊 Resultados del Crawling                                   ║
╠════════════════════════════════════════════════════════════════╣
║                                                                ║
║  ┌──────────────────────────────────────────────────────────┐ ║
║  │ 📁 Carpeta base: [Proyecto]/App_Data/crawlings/         │ ║
║  └──────────────────────────────────────────────────────────┘ ║
║                                                                ║
║  Dominios procesados:                                         ║
║  ┌──────────────────────────────────────────────────────────┐ ║
║  │ ✓ elmuseocanario.com → 47 páginas en                    │ ║
║  │   [Proyecto]/App_Data/crawlings/elmuseocanario_com/     │ ║
║  ├──────────────────────────────────────────────────────────┤ ║
║  │ ✓ cultura.grancanaria.com → 23 páginas en              │ ║
║  │   [Proyecto]/App_Data/crawlings/cultura_grancanaria...  │ ║
║  ├──────────────────────────────────────────────────────────┤ ║
║  │ ✓ memoriadelanzarote.com → 35 páginas en                │ ║
║  │   [Proyecto]/App_Data/crawlings/memoriadelanzarote_com/ │ ║
║  └──────────────────────────────────────────────────────────┘ ║
║                                                                ║
║  ┌─────────────────────────────────┐                         ║
║  │ ← Volver al formulario           │                         ║
║  └─────────────────────────────────┘                         ║
║                                                                ║
╚════════════════════════════════════════════════════════════════╝
```

---

## 🔄 Diagrama de Flujo

### Resolución de Rutas

```
┌─────────────────────────┐
│ Usuario introduce ruta  │
│ (o deja vacío)          │
└────────────┬────────────┘
             │
             ▼
    ┌────────────────────┐
    │ ¿Está vacío?       │
    └──┬────────────┬────┘
       │            │
      SÍ            NO
       │            │
       ▼            ▼
  ┌─────────────┐  ┌──────────────────────────┐
  │ Usar        │  │ ¿Contiene : o comienza  │
  │ App_Data/   │  │ con / o \ ?             │
  │ crawlings/  │  └──┬──────────────────┬───┘
  └──────┬──────┘     │                  │
         │          SÍ (Absoluta)      NO (Relativa)
         │            │                  │
         │            ▼                  ▼
         │      ┌──────────────────┐  ┌──────────────────┐
         │      │ Usar tal cual    │  │ Resolver desde   │
         │      │ D:\MisCrawls\    │  │ raíz proyecto    │
         │      └────────┬─────────┘  │ ~/MisCrawls/     │
         │               │            └────────┬─────────┘
         │               │                     │
         └───────────────┼─────────────────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ Intentar crear       │
              │ carpeta              │
              └──┬────────────┬──────┘
                 │            │
              ÉXITO         ERROR
                 │            │
                 ▼            ▼
            ┌─────────┐  ┌──────────────┐
            │ Guardar │  │ Mostrar error│
            │ archivos│  │ al usuario   │
            └─────────┘  └──────────────┘
```

---

## 💾 Estructura de Carpetas

### Por Defecto (sin input)

```
rag_canarias (Proyecto)
│
├── App_Data/
│   └── crawlings/ ◄──── CARPETA POR DEFECTO
│       ├── elmuseocanario_com/
│       │   ├── 00_index.txt
│       │   ├── 01_galeria.txt
│       │   ├── 02_horarios.txt
│       │   └── ...
│       │
│       ├── cultura_grancanaria_com/
│       │   ├── 00_index.txt
│       │   └── ...
│       │
│       └── memoriadelanzarote_com/
│           └── ...
│
└── ...
```

### Con Ruta Relativa Personalizada

```
rag_canarias (Proyecto)
│
├── App_Data/
│
├── MisCrawls/ ◄──── RUTA PERSONALIZADA
│   └── enero2024/
│       ├── elmuseocanario_com/
│       ├── cultura_grancanaria_com/
│       └── ...
│
└── ...
```

### Con Ruta Absoluta Personalizada

```
D:/
│
└── misCrawls/
    └── produccion/ ◄──── RUTA ABSOLUTA
        ├── elmuseocanario_com/
        ├── cultura_grancanaria_com/
        └── ...

(Nota: Completamente fuera del proyecto)
```

---

## 🧪 Casos de Uso Visuales

### Caso 1: Estudiante (recomendado)

```
ENTRADA:
├─ URL: https://elmuseocanario.com
├─ Carpeta: (DEJAR VACÍO)
├─ MaxPages: 50
├─ MaxDepth: 2
└─ FullCrawl: ✗

RESULTADO:
├─ Carpeta: [Proyecto]/App_Data/crawlings/
├─ Archivos: App_Data/crawlings/elmuseocanario_com/00_index.txt
│            App_Data/crawlings/elmuseocanario_com/01_galeria.txt
│            ...
└─ Estado: ✓ Éxito

VENTAJAS:
✓ No requiere configuración
✓ Fácil de encontrar
✓ Portátil con el proyecto
✓ Fácil de empaquetar para entregar
```

### Caso 2: Investigador Multi-Proyecto

```
ENTRADA 1:
├─ URL: (vacío - seeds por defecto)
├─ Carpeta: Investigacion/Enero
├─ MaxPages: 100
└─ MaxDepth: 3

RESULTADO 1:
├─ Carpeta: [Proyecto]/Investigacion/Enero/
└─ Archivos: Investigacion/Enero/dominio_1/...
             Investigacion/Enero/dominio_2/...

---

ENTRADA 2:
├─ URL: https://otro-sitio.es
├─ Carpeta: Investigacion/Febrero
├─ MaxPages: 100
└─ MaxDepth: 3

RESULTADO 2:
├─ Carpeta: [Proyecto]/Investigacion/Febrero/
└─ Archivos: Investigacion/Febrero/otro_sitio_es/...

VENTAJAS:
✓ Organización temporal clara
✓ Fácil comparación mes a mes
✓ Escalable
✓ Historial mantenido
```

### Caso 3: Servidor de Producción

```
ENTRADA:
├─ URL: https://patrimonio-canarias.es
├─ Carpeta: D:\crawls\produccion\patrimonio
├─ MaxPages: 5000
└─ FullCrawl: ✓

RESULTADO:
├─ Carpeta: D:\crawls\produccion\patrimonio\
├─ Archivos: D:\crawls\produccion\patrimonio\
│            patrimonio_canarias_es/00_index.txt
│            patrimonio_canarias_es/01_about.txt
│            ...
└─ Estado: ✓ Éxito (1000 páginas máximo)

VENTAJAS:
✓ Almacenamiento externo (más espacio)
✓ Mejor rendimiento en SSD rápido
✓ Separación clara del código
✓ Fácil backup
```

---

## ⚙️ Implementación Técnica

### Método ResolverRutaCarpeta() - Lógica de Decisión

```python
def resolver_ruta_carpeta(carpeta_personalizada):
    
    # Paso 1: Validación inicial
    if no_hay_input(carpeta_personalizada):
        return resolver("~/App_Data/crawlings/")
    
    # Paso 2: Limpiar entrada
    ruta_limpia = carpeta_personalizada.strip().strip("/").strip("\\")
    
    # Paso 3: Detectar tipo de ruta
    if contiene_absolute_markers(ruta_limpia):
        # Ruta absoluta: D:\carpeta\ o /home/user/carpeta
        return ruta_limpia + separador
    
    else:
        # Ruta relativa: carpeta/subcarpeta
        return resolver("~/" + ruta_limpia + "/")
```

### Método ObtenerRutaRelativa() - Visualización

```python
def obtener_ruta_relativa(ruta_absoluta):
    
    raiz_proyecto = obtener_raiz()
    
    if ruta_absoluta.starts_with(raiz_proyecto):
        # Dentro del proyecto: mostrar relativa
        relativa = ruta_absoluta.substring(len(raiz_proyecto))
        return f"[Proyecto]/{relativa.trim()}"
    else:
        # Fuera del proyecto: mostrar tal cual
        return ruta_absoluta
```

---

## 📋 Tabla Comparativa: Antes vs Después

### ANTES

```
Interfaz:
├─ URL: [_______]
├─ MaxPages: [50]
├─ MaxDepth: [2]
├─ FullCrawl: ☐
└─ [Iniciar]

Ruta por defecto:
├─ Hardcoded: C:\temp\crawler\
├─ Ubicación: Sistema (temporal)
└─ Problema: Se borra al limpiar %temp%

Resultados:
├─ "ejemplo.com -> 25 páginas guardadas en C:\temp\crawler\ejemplo_com"
└─ Ruta confusa y poco clara
```

### DESPUÉS

```
Interfaz:
├─ URL: [_______]
├─ 📁 Carpeta: [________] (defecto)
├─ MaxPages: [50]
├─ MaxDepth: [2]
├─ FullCrawl: ☐
└─ [Iniciar]

Ruta por defecto:
├─ Configurable: App_Data/crawlings/ (dentro proyecto)
├─ Ubicación: Proyecto (permanente)
├─ Ventaja: Portátil, fácil de empaquetar
└─ Flexible: Soporta relativas y absolutas

Resultados:
├─ Muestra carpeta base
├─ "✓ ejemplo.com → 25 páginas en [Proyecto]/App_Data/crawlings/ejemplo_com"
├─ Ruta clara y relativa
└─ Fácil de ubicar en Windows Explorer
```

---

## 🎯 Flujo Completo de Usuario

```
1. ABRIR APLICACIÓN
   └─ http://localhost:XXXX/Home/Index

2. VER FORMULARIO
   ├─ URL: (vacío)
   ├─ 📁 Carpeta: (vacío → usa por defecto)
   ├─ MaxPages: 50
   ├─ MaxDepth: 2
   └─ [Iniciar crawling]

3. EJECUTAR CRAWLING
   ├─ Resuelve ruta: App_Data/crawlings/
   ├─ Crea carpeta: App_Data\crawlings\elmuseocanario_com\
   ├─ Descarga páginas: 50
   ├─ Limpia HTML y guarda .txt
   └─ Proceso: ~25 segundos (con delays politeness)

4. VER RESULTADOS
   ├─ 📁 Carpeta base: [Proyecto]/App_Data/crawlings/
   ├─ ✓ elmuseocanario.com → 50 páginas en
   │    [Proyecto]/App_Data/crawlings/elmuseocanario_com
   └─ [← Volver]

5. ACCEDER A ARCHIVOS
   ├─ Windows Explorer:
   │  └─ C:\Users\Luis\source\repos\rag_canarias\App_Data\crawlings\
   ├─ O desde terminal:
   │  └─ cd App_Data\crawlings\elmuseocanario_com
   └─ Ver archivos: 00_index.txt, 01_galeria.txt, etc.
```

---

## 🎨 Ejemplos de Rutas Válidas

### Relativas (recomendadas)
```
✓ MisCrawls
✓ MisCrawls/enero
✓ MisCrawls/enero/2024
✓ Crawls/clientes/cliente_a
✓ Investigacion/enero_2024
✓ Backups/crawls_v1
```

### Absolutas (en Windows)
```
✓ D:\MisCrawls
✓ D:\MisCrawls\enero
✓ E:\BACKUP\crawls
✓ \\servidor\compartida\crawls
```

### Absolutas (en Linux/Mac)
```
✓ /home/usuario/crawls
✓ /home/usuario/investigacion/enero
✓ /mnt/external/crawls
```

### ❌ NO válidas
```
✗ ../../../etc/passwd (fuera del proyecto)
✗ C:\Windows\System32 (sin permisos)
✗ /etc/shadow (permisos del sistema)
✗ CON: (nombres reservados en Windows)
✗ nul, prn, lpt1, etc (dispositivos Windows)
```

---

## ✅ Estado Final

### Funcionalidades Implementadas
- ✅ Campo de entrada para carpeta en la UI
- ✅ Ruta por defecto: App_Data/crawlings
- ✅ Soporta rutas relativas
- ✅ Soporta rutas absolutas
- ✅ Validación y creación de carpetas
- ✅ Mensajes de error claros
- ✅ Rutas relativas en resultados
- ✅ Documentación completa

### Tests Manuales Realizados
- ✅ Dejar vacío → usa App_Data/crawlings
- ✅ Ruta relativa → crea dentro del proyecto
- ✅ Ruta absoluta → crea en ruta especificada
- ✅ Error en permisos → muestra mensaje claro
- ✅ Resultados muestra ruta base
- ✅ Compilación sin errores

---

**Documento Visual: Gestión de Carpetas Configurables v1.0**
