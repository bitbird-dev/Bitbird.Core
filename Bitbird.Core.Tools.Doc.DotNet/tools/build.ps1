param(
	[string] $InputDir,
	[string] $ConfigFile,
	[string] $OutputArchive = "Documentation.zip",
	[string] $TempDir = "bin/docfx_build"
)

$ErrorActionPreference = 'Stop'

try {
	Write-Host "Read Templates..";
	[string] $templateBuild = Get-Content ([System.IO.Path]::Combine($PSScriptRoot, "..", "resources", "build.json.tmpl")) -Raw;
	
	Write-Host "Read Config..";
	$config = Get-Content -Raw -Path $ConfigFile | ConvertFrom-Json;
	
	Write-Host "Create TempDir..";
	if (!(test-path ([System.IO.Path]::Combine($TempDir, "build")))) {
		New-Item -ItemType Directory -Force -Path ([System.IO.Path]::Combine($TempDir, "build")) | Out-Null;
	}
	Write-Host "Clear TempDir..";
	Get-ChildItem -Path ([System.IO.Path]::Combine($TempDir, "build")) -File | Remove-Item | Out-Null;
	Get-ChildItem -Path ([System.IO.Path]::Combine($TempDir, "build")) -Directory | Remove-Item -Recurse -Force | Out-Null;
	
	Write-Host "Write docfx.build.json..";
	[string] $build = $templateBuild;
	$build = $build.Replace("{title}", $config.title);
	$build = $build.Replace("{footer}", $config.footer.Replace("{year}",[System.DateTime]::Now.Year.ToString()));
	$build = $build.Replace("{output}", [System.IO.Path]::Combine($TempDir, "build").Replace('\','/'));
	$build = $build.Replace("{input}", $InputDir.Replace('\','/'));

	[string] $buildConfigPath = [System.IO.Path]::Combine((Get-Location), "docfx.build.json");
	$build | Out-File $buildConfigPath -Encoding utf8 -NoNewline;
	
	Write-Host $build;
	
	Write-Host "Copy DocFxTemplate..";
	[string] $templatePath = [System.IO.Path]::Combine((Get-Location), "DocFxTemplate");
	if (!(test-path ($templatePath))) {
		New-Item -ItemType Directory -Force -Path ($templatePath) | Out-Null;
	}
	Copy-Item ([System.IO.Path]::Combine($PSScriptRoot, "..", "resources", "DocFxTemplate", "*")) -Destination $templatePath -Recurse -Force;
	
	Write-Host "Exec docfx..";
	$args = @($buildConfigPath);
	& "docfx.exe" $args;
	
	Write-Host "Delete docfx.build.json..";
	Remove-Item $buildConfigPath | Out-Null;
	Write-Host "Delete DocFxTemplate..";
	Remove-Item $templatePath -Recurse -Force | Out-Null;
	
	Write-Host "Write OutputArchive..";
	if (test-path ($OutputArchive)) {
		Remove-item $OutputArchive | Out-Null;
	}
	Compress-Archive -Path ([System.IO.Path]::Combine($TempDir, "build", "*")) -DestinationPath $OutputArchive;
}
catch {
	throw;
}