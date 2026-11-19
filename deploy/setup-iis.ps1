# From the installer, names changed. Run as administrator.
# $thumbprint is the certificate from create-certificate.ps1.
$name = "Stockroom"
$path = "C:\Program Files\Stockroom\app"
$thumbprint = $args[0]

Add-Type -Path `
  "$env:windir\System32\inetsrv\Microsoft.Web.Administration.dll"
$sm = New-Object Microsoft.Web.Administration.ServerManager

$pool = $sm.ApplicationPools[$name]
if ($pool -eq $null) {
    $pool = $sm.ApplicationPools.Add($name)
}

$pool.ManagedRuntimeVersion = ""
$pool.StartMode = "AlwaysRunning"
$pool.ProcessModel.IdleTimeout = [TimeSpan]::Zero
$pool.Recycling.PeriodicRestart.Time = [TimeSpan]::Zero
$pool.ProcessModel.LoadUserProfile = $true

$site = $sm.Sites[$name]
if ($site -eq $null) {
    $site = $sm.Sites.Add($name, "http", "*:80:", $path)
}

$cert = Get-Item "Cert:\LocalMachine\My\$thumbprint"
$site.Bindings.Add("*:443:", $cert.GetCertHash(), "My").
    SetAttributeValue("sslFlags", 0)

$site.Applications["/"].ApplicationPoolName = $name
$site.Applications["/"].VirtualDirectories["/"].PhysicalPath = $path
$site.ServerAutoStart = $true
$sm.CommitChanges()
