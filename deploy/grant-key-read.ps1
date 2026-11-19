# From the installer, names changed. Windows service only.
# A type name cannot break across lines, hence "using namespace".
using namespace System.Security.Cryptography.X509Certificates

# $cert is the certificate from create-certificate.ps1.
$cert = $args[0]

$key = [RSACertificateExtensions]::GetRSAPrivateKey($cert)
$file = Join-Path $env:ProgramData `
    "Microsoft\Crypto\Keys\$($key.Key.UniqueName)"
icacls $file /grant "*S-1-5-19:R" /Q
