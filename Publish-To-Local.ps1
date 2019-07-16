param(
	[string] $Dir = $null
)

if ([string]::IsNullOrWhitespace($Dir)) {
	$Dir = "{0}\source\local nuget" -f $env:USERPROFILE;
}

Write-Host ("Publishing to local nuget dir: {0}" -f $Dir);

if (!(Test-Path $Dir)) {
	New-Item -ItemType Directory -Path $Dir;
}

[string[]] $packages = Get-ChildItem *.nupkg -Recurse | ?{ !$_.FullName.Contains("packages\") } | %{ $_.FullName };

foreach ($package in $packages) {
	$newPath = [System.IO.Path]::Combine($Dir, [System.IO.Path]::GetFilename($package));
	if (![System.IO.File]::Exists($newPath)) {
		Write-Host ("Publishing {0}" -f ([System.IO.Path]::GetFilename($package)));
		Copy-Item $package $Dir;
	}
}