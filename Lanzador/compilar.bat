@echo off
rem Recompila IniciarConsultorio.exe (lo deja en la raiz del repo). Usa el compilador que ya viene con Windows.
"%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /out:"%~dp0..\IniciarConsultorio.exe" /r:System.ServiceProcess.dll "%~dp0IniciarConsultorio.cs"
