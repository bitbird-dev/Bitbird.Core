[string] $wd = Get-Location;
[string] $sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe";
[string] $targetNugetExe = [System.IO.Path]::Combine($wd, "nuget.exe");
Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe;
Set-Alias nuget $targetNugetExe -Scope Global -Verbose;