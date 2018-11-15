@echo off
cls

.paket\paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)

fake-runner\fake.exe run build.fsx %*