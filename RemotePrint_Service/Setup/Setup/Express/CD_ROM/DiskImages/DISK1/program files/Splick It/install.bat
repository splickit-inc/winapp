net stop SplickItRemotePrint
installutil RemotePrint_Service.exe /u
rd c:\Splickit /s /q
xcopy *.* c:\Splickit\*.* /e /k /i /c /y
cd \Splickit
installutil RemotePrint_Service.exe /i
net start SplickItRemotePrint
RemotePrint_Desktop.exe