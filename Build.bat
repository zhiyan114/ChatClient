#-p:PublishReadyToRun=true
echo off
cls
dotnet publish -c Release -r win-x64 -p:PublishReadyToRun=true --self-contained=true "Chat Client.sln"
dotnet publish -c Release -r linux-x64 --self-contained=true "Chat Client.sln"
pause