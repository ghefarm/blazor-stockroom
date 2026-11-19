# From the installer, names changed. Run as administrator.
$subject = "CN=Stockroom"
$net = [System.Net.NetworkInformation.IPGlobalProperties]::
    GetIPGlobalProperties()
$fqdn = if ($net.DomainName) {
    "$($net.HostName).$($net.DomainName)"
} else { "" }
$names = @($env:COMPUTERNAME, $fqdn, "localhost") |
    Where-Object { $_ } |
    ForEach-Object { $_.ToLowerInvariant() } |
    Select-Object -Unique

$cert = New-SelfSignedCertificate -Subject $subject `
    -DnsName $names -CertStoreLocation Cert:\LocalMachine\My `
    -NotAfter (Get-Date).AddYears(10) `
    -KeyAlgorithm RSA -KeyLength 2048 `
    -KeyExportPolicy NonExportable `
    -Provider "Microsoft Software Key Storage Provider" `
    -FriendlyName "Stockroom HTTPS"

$root = New-Object `
  System.Security.Cryptography.X509Certificates.X509Store(
    "Root", "LocalMachine")
$root.Open("ReadWrite")
$root.Add((New-Object `
  System.Security.Cryptography.X509Certificates.X509Certificate2(
    , $cert.RawData)))
$root.Close()
