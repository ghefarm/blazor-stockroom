# From the installer, names changed. Windows service variant:
# Local Service (S-1-5-19) may modify the data folder.
icacls "C:\ProgramData\Stockroom" /grant "*S-1-5-19:(OI)(CI)M" /Q
