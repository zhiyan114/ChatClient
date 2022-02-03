echo off
cls

REM Inital Setup
mkdir TempPKG
cd TempPKG

REM Complete Windows PKG
Xcopy /E /I "..\Client\bin\Release\net5.0\win-x64\publish\" Client/
Xcopy /E /I "..\Server\bin\Release\net5.0\win-x64\publish\" Server/
tar -a -c -f ../Windows-x64.zip Client/* Server/*
del /F /Q Client
del /F /Q Server

REM Complete Linux PKG
Xcopy /E /I "..\Client\bin\Release\net5.0\linux-x64\publish" Client/
Xcopy /E /I "..\Server\bin\Release\net5.0\linux-x64\publish" Server/
tar -a -c -f ../Linux-x64.zip Client/* Server/*

REM Cleanup
cd ..
del /F /Q TempPKG
pause