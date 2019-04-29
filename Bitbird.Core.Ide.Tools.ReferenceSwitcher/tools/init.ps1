param($installPath, $toolsPath, $package, $project)

if ($PSVersionTable.PSVersion.Major -lt 3) {
    throw "PowerShell version $($PSVersionTable.PSVersion) is not supported. Please upgrade PowerShell to 3.0 or greater and restart Visual Studio."
}
else {
	Import-Module (Join-Path $toolsPath 'ReferenceSwitcher.psm1') -DisableNameChecking
}