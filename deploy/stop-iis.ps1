# From the installer, names changed. Run as administrator.
Add-Type -Path `
  "$env:windir\System32\inetsrv\Microsoft.Web.Administration.dll"
$sm = New-Object Microsoft.Web.Administration.ServerManager
$pool = $sm.ApplicationPools["Stockroom"]

$ids = @($pool.WorkerProcesses | ForEach-Object { $_.ProcessId })
$pool.Stop() | Out-Null
for ($i = 0; $i -lt 30 -and $pool.State -ne "Stopped"; $i++) {
    Start-Sleep -Seconds 1
}

if ($ids.Count -gt 0) {
    Wait-Process -Id $ids -Timeout 90 -ErrorAction SilentlyContinue
}
