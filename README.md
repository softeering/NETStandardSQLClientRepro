# NETStandardSQLClientRepro

## Issue

System.Data.SqlClient does not work when loading the external library (.NET Standard DLL) at runtime through reflection except if the library has been published for a specific platform (rid)

You can have a look at the .\build.ps1 script. If you build / publish the library using this command (notice the -r argument):

```powershell
dotnet publish -c release -r "win10-x64" -o ..\output ClassLibrary1.Implementation\ClassLibrary1.Implementation.csproj
```

The ConsoleApp1 will work but won't if you run the .\build.ps1 script with this (no RID defined):

```powershell
dotnet publish -c release -o ..\output ClassLibrary1.Implementation\ClassLibrary1.Implementation.csproj
```

## Steps to reproduce

- clone this repo
- run the .\build.ps1 script which will generate a folder called "output" containing the binaries
- open the solution in VS 2017 and press F5
  - running the ConsoleApp1 will work or not depending on the build you ran as described above
