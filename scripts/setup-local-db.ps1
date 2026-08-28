[CmdletBinding()]
param(
    [string]$ServerInstance = $(if ($env:LEAVEFLOW_SQL_SERVER) { $env:LEAVEFLOW_SQL_SERVER } else { "localhost,1433" }),
    [string]$Database = $(if ($env:LEAVEFLOW_SQL_DATABASE) { $env:LEAVEFLOW_SQL_DATABASE } else { "LeaveFlow" }),
    [string]$Username = $(if ($env:LEAVEFLOW_SQL_USER) { $env:LEAVEFLOW_SQL_USER } else { "sa" }),
    [string]$PasswordEnvironmentVariable = "LEAVEFLOW_SQL_PASSWORD",
    [string]$DockerContainer = $(if ($env:LEAVEFLOW_SQL_DOCKER_CONTAINER) { $env:LEAVEFLOW_SQL_DOCKER_CONTAINER } else { "" })
)

$ErrorActionPreference = "Stop"

if ($Database -notmatch "^[A-Za-z_][A-Za-z0-9_]*$") {
    throw "Database name '$Database' is not supported by this development setup script."
}

$password = [Environment]::GetEnvironmentVariable($PasswordEnvironmentVariable)
if ([string]::IsNullOrWhiteSpace($password)) {
    throw "Set `$env:$PasswordEnvironmentVariable before running this script."
}

$useDockerSqlCmd = -not [string]::IsNullOrWhiteSpace($DockerContainer)
$sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
if (-not $useDockerSqlCmd -and $null -eq $sqlcmd) {
    throw "sqlcmd was not found. Install Microsoft sqlcmd tools or run this from a shell where sqlcmd is available."
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$dbRoot = Join-Path $repoRoot "db"
$scriptFolders = @(
    "001_Tables",
    "002_Indexes",
    "003_StoredProcedures",
    "004_Seed"
)

function Invoke-SqlCmdFile {
    param(
        [string]$TargetDatabase,
        [string]$InputFile
    )

    if ($useDockerSqlCmd) {
        Get-Content -LiteralPath $InputFile -Raw |
            docker exec -i $DockerContainer /opt/mssql-tools18/bin/sqlcmd -S localhost -U $Username -P $password -d $TargetDatabase -b -I -C
    }
    else {
        & sqlcmd -S $ServerInstance -U $Username -P $password -d $TargetDatabase -b -I -i $InputFile
    }

    if ($LASTEXITCODE -ne 0) {
        throw "sqlcmd failed while applying $InputFile."
    }
}

function Invoke-SqlCmdQuery {
    param(
        [string]$TargetDatabase,
        [string]$Query
    )

    if ($useDockerSqlCmd) {
        & docker exec $DockerContainer /opt/mssql-tools18/bin/sqlcmd -S localhost -U $Username -P $password -d $TargetDatabase -b -I -C -Q $Query
    }
    else {
        & sqlcmd -S $ServerInstance -U $Username -P $password -d $TargetDatabase -b -I -Q $Query
    }

    if ($LASTEXITCODE -ne 0) {
        throw "sqlcmd failed while executing a setup query."
    }
}

if ($useDockerSqlCmd) {
    Write-Host "Ensuring database '$Database' exists in Docker container '$DockerContainer'."
}
else {
    Write-Host "Ensuring database '$Database' exists on '$ServerInstance'."
}
Invoke-SqlCmdQuery -TargetDatabase "master" -Query "IF DB_ID(N'$Database') IS NULL CREATE DATABASE [$Database];"

foreach ($folder in $scriptFolders) {
    $folderPath = Join-Path $dbRoot $folder
    if (-not (Test-Path $folderPath)) {
        throw "Expected database script folder was not found: $folderPath"
    }

    Write-Host "Applying $folder scripts."
    Get-ChildItem -Path $folderPath -Filter "*.sql" |
        Sort-Object Name |
        ForEach-Object {
            Write-Host "  $($_.Name)"
            Invoke-SqlCmdFile -TargetDatabase $Database -InputFile $_.FullName
        }
}

Write-Host "LeaveFlow local database setup completed."
