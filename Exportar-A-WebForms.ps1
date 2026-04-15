#!/usr/bin/env powershell
# Script para exportar componentes del crawler a un proyecto Web Forms
# Uso: .\Exportar-A-WebForms.ps1 -DestinationProject "C:\ruta\a\tu\WebFormProject"

param(
    [Parameter(Mandatory=$true, HelpMessage="Ruta del proyecto Web Forms destino")]
    [string]$DestinationProject,
    
    [switch]$InstalarNuGet = $false
)

# Colores para output
$Green = [System.ConsoleColor]::Green
$Red = [System.ConsoleColor]::Red
$Yellow = [System.ConsoleColor]::Yellow
$Blue = [System.ConsoleColor]::Cyan

function Write-Success { Write-Host $args[0] -ForegroundColor $Green }
function Write-Error-Custom { Write-Host $args[0] -ForegroundColor $Red }
function Write-Warning { Write-Host $args[0] -ForegroundColor $Yellow }
function Write-Info { Write-Host $args[0] -ForegroundColor $Blue }

Write-Info "==========================================="
Write-Info "   Exportador: MVC → Web Forms"
Write-Info "==========================================="
Write-Info ""

# Validar que la carpeta destino existe
if (-not (Test-Path $DestinationProject)) {
    Write-Error-Custom "ERROR: La carpeta destino no existe: $DestinationProject"
    exit 1
}

# Variables de rutas
$SourceMvc = Split-Path -Parent $MyInvocation.MyCommand.Path
$SourceServices = Join-Path $SourceMvc "rag_canarias\Services"
$DestServices = Join-Path $DestinationProject "Services"

Write-Info "Rutas:"
Write-Info "  ORIGEN (MVC): $SourceServices"
Write-Info "  DESTINO (WebForms): $DestServices"
Write-Info ""

# 1. Crear carpeta Services si no existe
Write-Info "[1/4] Creando carpeta Services..."
if (-not (Test-Path $DestServices)) {
    New-Item -ItemType Directory -Path $DestServices -Force | Out-Null
    Write-Success "  ✓ Carpeta Services creada"
} else {
    Write-Info "  ℹ Carpeta Services ya existe"
}

# 2. Copiar archivos
Write-Info "[2/4] Copiando archivos..."

$archivos = @("CrawlerService.cs", "PathHelper.cs")
$todoOk = $true

foreach ($archivo in $archivos) {
    $origen = Join-Path $SourceServices $archivo
    $destino = Join-Path $DestServices $archivo
    
    if (Test-Path $origen) {
        Copy-Item -Path $origen -Destination $destino -Force
        Write-Success "  ✓ $archivo copiado"
    } else {
        Write-Error-Custom "  ✗ No se encontró: $archivo"
        $todoOk = $false
    }
}

if (-not $todoOk) {
    exit 1
}

# 3. Crear carpeta App_Data si no existe
Write-Info "[3/4] Creando estructura App_Data..."
$DestAppData = Join-Path $DestinationProject "App_Data"

if (-not (Test-Path $DestAppData)) {
    New-Item -ItemType Directory -Path $DestAppData -Force | Out-Null
    Write-Success "  ✓ Carpeta App_Data creada"
} else {
    Write-Info "  ℹ Carpeta App_Data ya existe"
}

$DestCrawlings = Join-Path $DestAppData "crawlings"
if (-not (Test-Path $DestCrawlings)) {
    New-Item -ItemType Directory -Path $DestCrawlings -Force | Out-Null
    Write-Success "  ✓ Carpeta App_Data/crawlings creada"
} else {
    Write-Info "  ℹ Carpeta App_Data/crawlings ya existe"
}

# 4. Resumen
Write-Info "[4/4] Resumen:"
Write-Success ""
Write-Success "✓ Exportación completada exitosamente"
Write-Success ""
Write-Info "Archivos copiados:"
Write-Info "  • CrawlerService.cs"
Write-Info "  • PathHelper.cs"
Write-Info ""
Write-Info "Carpetas creadas:"
Write-Info "  • $DestServices"
Write-Info "  • $DestAppData"
Write-Info ""

# 5. Próximos pasos
Write-Info "PRÓXIMOS PASOS:"
Write-Info ""
Write-Info "1. Abre tu proyecto Web Forms en Visual Studio"
Write-Info ""
Write-Info "2. Instala HtmlAgilityPack (si no lo tienes):"
Write-Info "   Tools → NuGet Package Manager → Package Manager Console"
Write-Info "   Install-Package HtmlAgilityPack"
Write-Info ""
Write-Info "3. En tu code-behind, agrega:"
Write-Info "   using [TuNamespace].Services;"
Write-Info ""
Write-Info "4. Usa el crawler:"
Write-Info "   var crawler = new CrawlerService();"
Write-Info "   var resultado = crawler.CrawlDominio(url, carpeta, maxPages, maxDepth);"
Write-Info ""
Write-Info "5. Lee la guía completa:"
Write-Info "   $SourceMvc\GUIA_WEBFORMS.md"
Write-Info ""

Write-Success "==========================================="
Write-Success "    ¡Listo para usar en Web Forms!"
Write-Success "==========================================="
