
cd $PSScriptRoot

if (Test-Path output)
{
	rmdir -Recurse -Force output
	Sleep -Seconds 5
}

mkdir output

# WORKS
# dotnet publish -c release -r "win10-x64" -o ..\output ClassLibrary1.Implementation\ClassLibrary1.Implementation.csproj
# DOESN'T WORK
dotnet publish -c release -o ..\output ClassLibrary1.Implementation\ClassLibrary1.Implementation.csproj
