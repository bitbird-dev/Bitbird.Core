param(
	[string] $ConfigFile,
	[string] $OutputDir,
	[string] $TempDir = "bin/docfx"
)

$ErrorActionPreference = 'Stop'

try {
	Write-Host "Read Templates..";
	[string] $templateMetaData = Get-Content ([System.IO.Path]::Combine($PSScriptRoot, "..", "resources", "export.metadata.json.tmpl")) -Raw;
	[string] $templateMetaDataItem = Get-Content ([System.IO.Path]::Combine($PSScriptRoot, "..", "resources", "export.metadata.item.json.tmpl")) -Raw;

	Write-Host "Read Config..";
	$configContent = Get-Content -Path $ConfigFile;
	$config = $configContent | ConvertFrom-Json;
	
	Write-Host "Create TempDir..";
	if (!(test-path ([System.IO.Path]::Combine($TempDir, "export")))) {
		New-Item -ItemType Directory -Force -Path ([System.IO.Path]::Combine($TempDir, "export")) | Out-Null;
	}
	Write-Host "Clear TempDir..";
	Get-ChildItem -Path ([System.IO.Path]::Combine($TempDir, "export")) -File | Remove-Item | Out-Null;
	Get-ChildItem -Path ([System.IO.Path]::Combine($TempDir, "export")) -Directory | Remove-Item -Recurse -Force | Out-Null;

	Write-Host "Write docfx.metadata.json..";
	[string] $items = "";
	foreach ($api in $config.api) {
		[string] $item = $templateMetaDataItem;
		$item = $item.Replace("{csproj}", $api.csproj);
		$item = $item.Replace("{output}", [System.IO.Path]::Combine($TempDir, "export", $api.output).Replace('\', '/'));
		$items = $items + $item;
	}

	[string] $metaData = $templateMetaData;
	$metaData = $metaData.Replace("{entries}", $items);

	[string] $metaDataConfigPath = [System.IO.Path]::Combine((Get-Location), "docfx.metadata.json");
	$metaData | Out-File $metaDataConfigPath -Encoding utf8 -NoNewline;
	
	Write-Host $metaData;

	Write-Host "Copy filterConfig.yml..";
	[string] $filterPath = [System.IO.Path]::Combine((Get-Location), "filterConfig.yml");
	Copy-Item ([System.IO.Path]::Combine($PSScriptRoot, "..", "resources", "filterConfig.yml")) -Destination $filterPath -Force | Out-Null;

	Write-Host "Exec docfx..";
	$args = @($metaDataConfigPath);
	$docFxPath = [System.IO.Path]::Combine([Environment]::GetEnvironmentVariable("ChocolateyInstall"), "bin", "docfx.exe");
	Write-Host ("  " + $docFxPath);
	& ($docFxPath) $args;

	Write-Host "Delete docfx.metadata.json..";
	Remove-Item $metaDataConfigPath | Out-Null;
	Write-Host "Delete filterConfig.yml..";
	Remove-Item $filterPath | Out-Null;

	Write-Host "Copy content..";
	foreach ($content in $config.content) {
		[string] $dst = [System.IO.Path]::Combine($TempDir, "export", $content.dst);
		if (!(test-path ($dst))) {
			Write-Host "  Create $($dst)";
			New-Item -ItemType Directory -Force -Path ($dst) | Out-Null;
		}
		[string] $from = ([System.IO.Path]::Combine((Get-Location), $content.src, "*"));
		Write-Host "  Copy $($from) to $($dst)";
		Copy-Item $from -Destination $dst -Recurse -Force | Out-Null;
	}

	Write-Host "Create OutputDir..";
	if (!(test-path ($OutputDir))) {
		New-Item -ItemType Directory -Force -Path ($OutputDir) | Out-Null;
	}
	Write-Host "Clear OutputDir..";
	Get-ChildItem -Path $OutputDir -File | Remove-Item | Out-Null;
	Get-ChildItem -Path $OutputDir -Directory | Remove-Item -Recurse -Force | Out-Null;
	
	Write-Host "Copy results to OutputDir..";
	Copy-Item ([System.IO.Path]::Combine(([System.IO.Path]::Combine($TempDir, "export")), "*")) -Destination $OutputDir -Recurse -Force | Out-Null;
}
catch {
	throw;
}