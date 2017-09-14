# NETStandardSQLClientRepro

## Issue

System.Data.SqlClient does not work when loading the external library (.NET Standard DLL) at runtime through reflection except if the library has been published for a specific platform (rid).
Trying to execute a SQL query against a SQL Server instance throws "System.PlatformNotSupportedException: System.Data.SqlClient is not supported on this platform" exception.

You can have a look at the .\build.ps1 script. If you build / publish the library using this command (notice the -rid argument):

```powershell
.\build.ps1 -rid "win10-x64"
```

The ConsoleApp1 will work but won't if you run the .\build.ps1 script without any RID defined:

```powershell
.\build.ps1
```

Error message is: "System.PlatformNotSupportedException: System.Data.SqlClient is not supported on this platform"

## Steps to reproduce

### pre-requisites
- .NET Core 2 installed
- you need an empty SQL Server instance available
- you can change the connection string in the class ClassLibrary1.Implementation.SQLServerReader

### how-to
- clone this repo
- run the .\build.ps1 script which will generate a folder called "output" containing the binaries
- open the solution in VS 2017 and press F5
  - running the ConsoleApp1 will work or not depending on the build you ran as described above

### my environment
- dotnet --info

```
.NET Command Line Tools (2.0.0)

Product Information:
 Version:            2.0.0
 Commit SHA-1 hash:  cdcd1928c9

Runtime Environment:
 OS Name:     Windows
 OS Version:  10.0.14393
 OS Platform: Windows
 RID:         win10-x64
 Base Path:   C:\Program Files\dotnet\sdk\2.0.0\

Microsoft .NET Core Shared Framework Host

  Version  : 2.0.0
  Build    : e8b8861ac7faf042c87a5c2f9f2d04c98b69f28d
```

- Visual Studio 2017 15.3.4
