# aver

[![Build Status](https://travis-ci.org/mocosha/assembly-version.svg?branch=master)](https://travis-ci.org/mocosha/assembly-version)

`aver` is the dotnet tool for reading assembly information.

### Installation

    dotnet tool install --global aver

### How To Use

    USAGE: aver.exe [--help] [--assembly] [--product] [--file] [--all] [<string>]
    PATH:
    
      <string>          Path to the assembly (by default only assembly version is printed)
      
    OPTIONS:
      
      --assembly, -a    Print assembly version
      --product, -p     Print product version
      --file, -f        Print file version
      --filename, -n    Print file name 
      --all, -a         Print whole assembly info
      --help            display this list of options.

### Why?

There is no easy way to read assembly information without writing some code.
