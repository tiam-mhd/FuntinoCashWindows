@echo off
setlocal
set "PROJECT_DIR=%~dp0.."
pushd "%PROJECT_DIR%"
where msbuild.exe >nul 2>&1
if errorlevel 1 (
  echo MSBuild.exe was not found. Install Visual Studio Build Tools with the .NET Framework 4.6.1 Developer Pack.
  popd
  exit /b 2
)
msbuild FuntinoControlCenter.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /verbosity:minimal
if errorlevel 1 (
  echo Build failed. Review the MSBuild output above.
  popd
  exit /b 1
)
if not exist "bin\Release\FuntinoControlCenter.exe" (
  echo Build reported success but the expected EXE was not produced.
  popd
  exit /b 3
)
echo Build succeeded: %PROJECT_DIR%\bin\Release\FuntinoControlCenter.exe
popd
exit /b 0
