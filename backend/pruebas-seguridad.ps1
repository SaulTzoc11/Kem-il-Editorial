# Pruebas de control de acceso por rol (solo lectura / sin crear datos).
# Uso (con el backend corriendo):  .\pruebas-seguridad.ps1
# Genera evidencia-seguridad.txt con el resultado.
param([string]$Base = "http://localhost:5229")

Start-Transcript -Path "evidencia-seguridad.txt" -Force | Out-Null

$Password = "Kemil2026!"
$usuarios = [ordered]@{
    Administrador = "admin@kemileditorial.com"
    Asesor        = "asesor@kemileditorial.com"
    Cliente       = "cliente@kemileditorial.com"
    Mayorista     = "mayorista@kemileditorial.com"
    Institucional = "institucional@kemileditorial.com"
}
$todos       = @("Administrador", "Asesor", "Cliente", "Mayorista", "Institucional")
$compradores = @("Cliente", "Mayorista", "Institucional")

function Login($correo) {
    for ($i = 0; $i -lt 3; $i++) {
        try {
            $cuerpo = @{ correo = $correo; password = $Password } | ConvertTo-Json
            $r = Invoke-RestMethod -Method Post -Uri "$Base/api/auth/login" -ContentType "application/json" -Body $cuerpo
            return $r.token
        } catch {
            $codigo = 0
            if ($_.Exception.Response) { $codigo = [int]$_.Exception.Response.StatusCode }
            if ($codigo -eq 429) {
                Write-Host "  Limite de intentos de login, esperando 65 s..." -ForegroundColor Yellow
                Start-Sleep -Seconds 65
            } else { return $null }
        }
    }
    return $null
}

function Invocar($metodo, $ruta, $token, $cuerpo) {
    $headers = @{}
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    $params = @{ Method = $metodo; Uri = "$Base$ruta"; Headers = $headers; UseBasicParsing = $true }
    if ($null -ne $cuerpo) { $params.Body = $cuerpo; $params.ContentType = "application/json" }
    try {
        $r = Invoke-WebRequest @params
        return [int]$r.StatusCode
    } catch {
        if ($_.Exception.Response) { return [int]$_.Exception.Response.StatusCode }
        return 0
    }
}

# P = roles con permiso. Un rol con permiso debe pasar la seguridad (cualquier codigo menos 401/403;
# 400 o 404 significa que paso y el cuerpo de prueba vacio se rechazo). Un rol sin permiso debe dar 403.
$pruebas = @(
    @{ N = "GET  /api/libros (publico)";                M = "GET"; R = "/api/libros"; B = $null; Pub = $true },
    @{ N = "GET  /api/auth/me";                         M = "GET"; R = "/api/auth/me"; B = $null; P = $todos },
    @{ N = "GET  /api/usuarios";                        M = "GET"; R = "/api/usuarios"; B = $null; P = @("Administrador") },
    @{ N = "GET  /api/roles";                           M = "GET"; R = "/api/roles"; B = $null; P = @("Administrador") },
    @{ N = "PUT  /api/usuarios/999999/estado";          M = "PUT"; R = "/api/usuarios/999999/estado"; B = '{"estado":true}'; P = @("Administrador") },
    @{ N = "POST /api/auth/usuarios";                   M = "POST"; R = "/api/auth/usuarios"; B = "{}"; P = @("Administrador") },
    @{ N = "GET  /api/admin/resumen";                   M = "GET"; R = "/api/admin/resumen"; B = $null; P = @("Administrador") },
    @{ N = "GET  /api/ordenes (monitoreo)";             M = "GET"; R = "/api/ordenes"; B = $null; P = @("Administrador") },
    @{ N = "GET  /api/ordenes/mis-pedidos";             M = "GET"; R = "/api/ordenes/mis-pedidos"; B = $null; P = $compradores },
    @{ N = "POST /api/ordenes";                         M = "POST"; R = "/api/ordenes"; B = '{"items":[]}'; P = $compradores },
    @{ N = "POST /api/cotizaciones";                    M = "POST"; R = "/api/cotizaciones"; B = "{}"; P = @("Mayorista", "Institucional") },
    @{ N = "GET  /api/cotizaciones/bandeja";            M = "GET"; R = "/api/cotizaciones/bandeja"; B = $null; P = @("Administrador", "Asesor") },
    @{ N = "PUT  /api/cotizaciones/999999/responder";   M = "PUT"; R = "/api/cotizaciones/999999/responder"; B = "{}"; P = @("Administrador", "Asesor") }
)

Write-Host "Iniciando sesion con los 5 roles..."
$tokens = @{}
foreach ($rol in $usuarios.Keys) {
    $t = Login $usuarios[$rol]
    if (-not $t) {
        Write-Host "No se pudo iniciar sesion como $rol ($($usuarios[$rol])). Revisa que el backend este corriendo." -ForegroundColor Red
        Stop-Transcript | Out-Null
        exit 1
    }
    $tokens[$rol] = $t
}

$filas = @()
$fallas = 0
foreach ($p in $pruebas) {
    $fila = [ordered]@{ Prueba = $p.N }

    $c = Invocar $p.M $p.R $null $p.B
    if ($p.Pub) { $ok = ($c -ne 401 -and $c -ne 403 -and $c -ne 0) } else { $ok = ($c -eq 401) }
    if (-not $ok) { $fallas++ }
    $fila["Sin token"] = "$c " + $(if ($ok) { "ok" } else { "MAL" })

    foreach ($rol in $usuarios.Keys) {
        $c = Invocar $p.M $p.R $tokens[$rol] $p.B
        $debePasar = ($p.Pub -eq $true) -or ($p.P -contains $rol)
        if ($debePasar) { $ok = ($c -ne 401 -and $c -ne 403 -and $c -ne 0) } else { $ok = ($c -eq 403) }
        if (-not $ok) { $fallas++ }
        $fila[$rol] = "$c " + $(if ($ok) { "ok" } else { "MAL" })
    }
    $filas += [pscustomobject]$fila
}

$filas | Format-Table -AutoSize | Out-String -Width 300 | Write-Host
Write-Host "Leyenda: 'ok' = comportamiento esperado. Sin token: 401. Rol sin permiso: 403."
Write-Host "Rol con permiso: cualquier codigo distinto de 401/403 (200 correcto; 400/404 = paso la seguridad y el cuerpo de prueba vacio fue rechazado)."
if ($fallas -eq 0) { Write-Host "RESULTADO: todas las pruebas pasaron." -ForegroundColor Green }
else { Write-Host "RESULTADO: $fallas prueba(s) con comportamiento inesperado (marcadas MAL)." -ForegroundColor Red }

Stop-Transcript | Out-Null