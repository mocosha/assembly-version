#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use .Net
  dotnet tool install fake-cli -g
  .paket/paket.exe restore
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi
  fake run build.fsx $@
else
  # use mono
  mono tool install fake-cli -g
  mono .paket/paket.exe restore
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi
  mono fake run build.fsx $@
fi