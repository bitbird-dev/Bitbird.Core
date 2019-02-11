param(
	[string] $ConfigFile,
	[string] $OutputDir,
	[string] $TempDir = "bin/docfx_build"
)

[string] $templateBuild = Get-Content ([System.IO.Path]::Combine($PSScriptRoot, "..", "resources", "build.json.tmpl")) -Raw;

$config = Get-Content -Raw -Path $ConfigFile | ConvertFrom-Json;

Get-ChildItem -Path $TempDir -File | Remove-Item;
Get-ChildItem -Path $TempDir -File | Remove-Item -Recurse;

[string] $build = $templateBuild;
$build = $build.Replace("{title}", $config.title);
$build = $build.Replace("{footer}", $config.footer.Replace("{year}",[System.DateTime]::Now.Year.ToString()));

[string] $buildConfigPath = [System.IO.Path]::Combine((Get-Location), "docfx.build.json");
$build | Out-File $buildConfigPath -Encoding utf8 -NoNewline;

[string] $templatePath = [System.IO.Path]::Combine((Get-Location), "DocFxTemplate");
Copy-Item ([System.IO.Path]::Combine($PSScriptRoot, "..", "resources", "DocFxTemplate", "*")) -Destination $templatePath -Recurse -Force;

$args = @($buildConfigPath);
& "docfx.exe" $args;

Remove-Item $buildConfigPath;
Remove-Item $templatePath -Recurse;

Get-ChildItem -Path $OutputDir -File | Remove-Item;
Get-ChildItem -Path $OutputDir -File | Remove-Item -Recurse;
Copy-Item ([System.IO.Path]::Combine($TempDir, "*")) -Destination $OutputDir -Recurse -Force;