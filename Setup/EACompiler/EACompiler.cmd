@echo off

rem ________________________________________________________________
rem
rem  Copy this file to the directory where your .eap files live then
rem  invoke it from there (e.g., by double-clicking on the file name
rem  in the file explorer. That will cause all .eap files in the
rem  directory to be compiled into .C files.
rem
rem  Note this shortcut will only work if you did the EACompiler
rem  installation process using the default path for the installed
rem  files (because that path is assumed here).
rem ________________________________________________________________

SET DEFAULT_INSTALLATION_DIR="C:\Program Files (x86)\ArrayPower\EACompiler"

%DEFAULT_INSTALLATION_DIR%\EACompilerConsole 

