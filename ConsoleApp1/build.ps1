param([string]$rid = $null)
cd $PSScriptRoot

if (Test-Path output)
{
	rmdir -Recurse -Force output
	Sleep -Seconds 5
}

mkdir output

if ($rid -eq $null -or $rid -eq "")
{
	# executing ConsoleApp1 won't work ()
	dotnet publish -c release -o ..\output ClassLibrary1.Implementation\ClassLibrary1.Implementation.csproj
}
else
{
	# executing ConsoleApp1 will work
	dotnet publish -c release -r $rid -o ..\output ClassLibrary1.Implementation\ClassLibrary1.Implementation.csproj
}
