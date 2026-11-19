# From the installer, names changed. Run as administrator.
# $subject and $names are built as in create-certificate.ps1.
function Test-Certificate($cert) {
    if ($cert.NotAfter -lt (Get-Date).AddYears(1)) {
        return $false
    }

    $present = @($cert.DnsNameList |
        ForEach-Object { $_.Unicode.ToLowerInvariant() })
    return -not ($names | Where-Object { $present -notcontains $_ })
}

$cert = Get-ChildItem Cert:\LocalMachine\My |
    Where-Object { $_.Subject -eq $subject } |
    Sort-Object NotAfter -Descending |
    Select-Object -First 1

if ($null -eq $cert -or -not (Test-Certificate $cert)) {
    & "$PSScriptRoot\create-certificate.ps1"
}
