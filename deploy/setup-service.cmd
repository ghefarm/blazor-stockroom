sc.exe create Stockroom ^
 binPath= "\"C:\Program Files\Stockroom\app\Stockroom.Web.exe\"" ^
 start= auto obj= "NT AUTHORITY\LocalService" password= "" ^
 DisplayName= "Stockroom"
sc.exe description Stockroom "Stock and order manager"
sc.exe failure Stockroom reset= 86400 ^
 actions= restart/60000/restart/60000/restart/60000
netsh advfirewall firewall add rule name="Stockroom" dir=in ^
 action=allow protocol=TCP localport=80,443
