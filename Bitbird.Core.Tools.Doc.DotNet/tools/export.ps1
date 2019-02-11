param(
	[string] $ConfigFile,
	[string] $OutputDir,
	[string] $TempDir = "bin/docfx"
)

[string] $templateMetaData = Get-Content ([System.IO.Path]::Combine($PSScriptRoot, "..", "resources", "export.metadata.json.tmpl")) -Raw;
[string] $templateMetaDataItem = Get-Content ([System.IO.Path]::Combine($PSScriptRoot, "..", "resources", "export.metadata.item.json.tmpl")) -Raw;

$config = Get-Content -Raw -Path $ConfigFile | ConvertFrom-Json;

Get-ChildItem -Path $TempDir -File | Remove-Item;
Get-ChildItem -Path $TempDir -File | Remove-Item -Recurse;

[string] $items = "";
foreach ($api in $config.api) {
	[string] $item = $templateMetaDataItem;
	$item = $item.Replace("{csproj}", $api.csproj);
	$item = $item.Replace("{output}", [System.IO.Path]::Combine($TempDir, $api.csproj));
	$items = $items + $item;
}

[string] $metaData = $templateMetaData;
$metaData = $metaData.Replace("{entries}", $items);

[string] $metaDataConfigPath = [System.IO.Path]::Combine((Get-Location), "docfx.metadata.json");
$metaData | Out-File $metaDataConfigPath -Encoding utf8 -NoNewline;

[string] $filterPath = [System.IO.Path]::Combine((Get-Location), "filterConfig.yml");
Copy-Item ([System.IO.Path]::Combine($PSScriptRoot, "..", "resources", "filterConfig.yml")) -Destination $filterPath -Force;

$args = @($metaDataConfigPath);
& "docfx.exe" $args;

Remove-Item $metaDataConfigPath;
Remove-Item $filterPath;

foreach ($content in $config.content) {
	[string] $dst = [System.IO.Path]::Combine((Get-Location), $content.dst);
    [System.IO.Directory]::CreateDirectory($dst);
	Copy-Item ([System.IO.Path]::Combine((Get-Location), $content.src, "*")) -Destination $dst -Recurse -Force;
}

Get-ChildItem -Path $OutputDir -File | Remove-Item;
Get-ChildItem -Path $OutputDir -File | Remove-Item -Recurse;
Copy-Item ([System.IO.Path]::Combine($TempDir, "*")) -Destination $OutputDir -Recurse -Force;