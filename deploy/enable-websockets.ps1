# From the installer, names changed. Run as administrator.
Enable-WindowsOptionalFeature -Online `
    -FeatureName IIS-WebSockets -All -NoRestart
