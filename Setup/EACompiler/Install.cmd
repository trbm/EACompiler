
@echo off

set PRODUCT=EACompiler
echo.
echo.   %PRODUCT% Installer (v1.3)
echo.
echo.   Removing existing versions of %PRODUCT% (if any)
echo.
WMIC product where name="%PRODUCT%" call uninstall > null
rem msiexec /uninstall %PRODUCT%.msi

set SWITCHES=/quiet 
echo.   Installing the latest version of %PRODUCT% (options: %SWITCHES%)
echo.
msiexec  %SWITCHES% /i %PRODUCT%.msi




  


