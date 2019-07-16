param(
    [parameter(Mandatory = $true)][string] $ProjectDir,
	[parameter(Mandatory = $true)][string] $ProjectPath,
	[parameter(Mandatory = $true)][string] $ProjectName,
	[parameter(Mandatory = $false)][switch] $IsTool = $false,
	[parameter(Mandatory = $false)][string] $VersionPostfix = ""
)

# Helper functions
function LogError {
	param(
		[parameter(Position=0)][string] $msg, 
		[parameter(Position=1)][string] $file
	)
	Write-Host ("{0}: error: {1}" -f $file, $msg);
}

function ReadPackTokens {
	param (
		[string] $file
	)

	[string] $dir = [System.IO.Path]::GetDirectoryName($file);
	[xml] $xml = Get-Content $file;

	[string] $tokens = "";

	foreach ($import in $xml.Project.Import.Project) {
		[string] $importFile = [System.IO.Path]::Combine($dir, $import);
		$tokens = $tokens + (ReadPackTokens -file $importFile);
	}

	foreach ($node in $xml.SelectNodes("/Project/PropertyGroup[not(@Condition)]/*")) {
		$tokens = '{0};{1}="{2}"' -f $tokens, $node.Name, $node.InnerXml;
	}

	return $tokens;
}

function EnsureNuspecExists {
	[string] $defaultNuspecFiles = '    <file src="publish\**" target="lib"/>';

	if ($IsTool) {
		$defaultNuspecFiles = '    <file src="publish\**" target="tools"/>';
	}

	[string] $defaultNuspec = '<?xml version="1.0" encoding="utf-8"?>
<package>
  <metadata>
    <id>$ProjectName$</id>
    <version>$VersionPrefix$$VersionPostfix$</version>
    <title>$Id$</title>
    <authors>$Authors$</authors>
    <owners>$Authors$</owners>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <description>$Description$</description>
    <copyright>$Copyright$</copyright>
  </metadata>
  <files>
{0}
  </files>
</package>' -f $defaultNuspecFiles;
	
	[string] $nuspecPath = ([System.IO.Path]::Combine($ProjectDir, ("{0}.nuspec" -f $ProjectName)));

	if (!(Test-Path $nuspecPath)) {
		Write-Output $defaultNuspec | Out-File $nuspecPath -Encoding utf8;
	}

	return $nuspecPath;
}

# Main function
function Main {
	[string] $ConfigurationName = "Release";

	[string] $outdir = ([System.IO.Path]::Combine($ProjectDir, "publish"));
	
	if (Test-Path $outdir) {
		Remove-Item ([System.IO.Path]::Combine($outdir, "*")) -Recurse -Force;
	}

	Write-Host "ProjectDir: " + $ProjectDir;

	[string[]] $params = @(
		"publish", $ProjectPath,
		"--configuration", $ConfigurationName,
		"--no-build",
		"--no-restore",
		"--output", $outdir
	)
	& "dotnet" $params;
	
	[string] $tokens = ReadPackTokens -file $ProjectPath;

	if ([string]::IsNullOrWhitespace($VersionPostfix)) {
		$VersionPostfix = "";
	}
	else {
		if (!($VersionPostfix.StartsWith("-"))) {
			$VersionPostfix = "-" + $VersionPostfix;
		}
	}
	
	[string[]] $params = @(
		"pack", (EnsureNuspecExists),
		"-OutputDirectory", $outdir,
		"-Properties", ('Configuration="{0}";ProjectName="{1}";VersionPostfix="{2}"{3}' -f $ConfigurationName, $ProjectName, $VersionPostfix, $tokens)
	)
	if ($IsTool){
		$params = $params += '-Tool';
	}
	& "nuget" $params;
}

# Safe execution
[bool] $hasError = $False;
try
{
	Main;
}
catch {
	LogError ("An error occurred during the creation of the package: {0}" -f $PSItem.ToString()) ("{0}({1},{2})" -f $PSCommandPath, $_.InvocationInfo.ScriptLineNumber, $_.InvocationInfo.OffsetInLine);
	$hasError = $True;
}

if ($hasError) {
	exit(10);
}
else {
	exit(0);
}