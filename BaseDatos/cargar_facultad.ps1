# Carga la base nueva en el servidor de la facultad (26.216.108.167).
# Espera a que el servidor conteste (el profesor lo prende a la manana) y corre
# los scripts en orden. Las credenciales las lee del Web.config
# (connection string "ConsultorioDB_facultad"), no hay que pegarlas aca.
#
# Uso manual:  powershell -ExecutionPolicy Bypass -File cargar_facultad.ps1

$repo   = Split-Path -Parent $PSScriptRoot
$log    = Join-Path $PSScriptRoot ("facultad_" + (Get-Date -Format "yyyyMMdd_HHmm") + ".log")
$config = [xml](Get-Content (Join-Path $repo "Consultorio2026\Web.config"))
$cs     = ($config.configuration.connectionStrings.add | Where-Object { $_.name -eq "ConsultorioDB_facultad" }).connectionString

$server = [regex]::Match($cs, "Data Source=([^;]+)").Groups[1].Value
$base   = [regex]::Match($cs, "Initial Catalog=([^;]+)").Groups[1].Value
$user   = [regex]::Match($cs, "User ID=([^;]+)").Groups[1].Value
$pass   = [regex]::Match($cs, "Password=([^;]+)").Groups[1].Value

function Escribir($texto) { $linea = (Get-Date -Format "HH:mm:ss") + "  " + $texto; $linea | Tee-Object -FilePath $log -Append }

Escribir "Esperando al servidor $server ..."
$intentos = 0
while (-not (Test-NetConnection $server -Port 1433 -InformationLevel Quiet -WarningAction SilentlyContinue)) {
    $intentos++
    if ($intentos -ge 18) {   # 3 horas
        Escribir "El servidor no contesto en 3 horas. No se cargo nada."
        msg * "Consultorio: el servidor de la facultad NO contesto. No se cargo la base."
        exit 1
    }
    Start-Sleep -Seconds 600
}
Escribir "Servidor disponible. Cargando la base $base ..."

# Si otra maquina del grupo ya la cargo, no se vuelve a correr (el 01 no se puede ejecutar dos veces)
$yaCargada = & sqlcmd -S $server -d $base -U $user -P $pass -C -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables WHERE name = 'PERSONA'" 2>&1
if ("$yaCargada".Trim() -eq "1") {
    Escribir "La base ya tiene el esquema nuevo (tabla PERSONA). No se carga de nuevo."
    msg * "Consultorio: la base de la facultad YA estaba cargada. No se hizo nada."
    exit 0
}

# 04 solo hace algo si existe la base vieja; 05 no trae SET QUOTED_IDENTIFIER, por eso el -I
$scripts = "01_esquema.sql", "02_datos.sql", "04_migracion.sql", "03_compatibilidad.sql",
           "05_datos_demo.sql", "06_usuarios_grupo.sql", "07_procedimientos.sql"

foreach ($s in $scripts) {
    Escribir "== $s"
    $salida = & sqlcmd -S $server -d $base -U $user -P $pass -C -I -b -i (Join-Path $PSScriptRoot $s) 2>&1
    $salida | Out-File -FilePath $log -Append -Encoding utf8
    if ($LASTEXITCODE -ne 0) {
        Escribir "ERROR en $s (ver el log). Se corta aca."
        msg * "Consultorio: fallo $s en la base de la facultad. Mira $log"
        exit 1
    }
}

$check = & sqlcmd -S $server -d $base -U $user -P $pass -C -W -Q "SET NOCOUNT ON; SELECT COUNT(*) AS usuarios FROM Usuario; SELECT COUNT(*) AS medicos FROM Medico;" 2>&1
$check | Out-File -FilePath $log -Append -Encoding utf8
Escribir "LISTO. Base de la facultad cargada."
msg * "Consultorio: base de la facultad CARGADA. Log: $log"
