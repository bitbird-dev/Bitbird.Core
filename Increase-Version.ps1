

$filename = [System.IO.Path]::Combine((Get-Location), "SharedProperties.props");
[xml] $props = Get-Content $filename -Encoding UTF8;
$version = $props.Project.PropertyGroup.versionprefix;
$parts = $version.Split(".");
$parts[2] = ([int]::Parse($parts[2]) + 1).ToString();
$newVersion = [string]::Join(".", $parts);
$props.Project.PropertyGroup.versionprefix = $newVersion;
$props.Save($filename);
Write-Host ("Updated {0} from {1} to {2}" -f $filename, $version, $newVersion);


$filename = [System.IO.Path]::Combine((Get-Location), "SharedAssemblyInfo.cs");
$cs = Get-Content $filename -Encoding UTF8 -Raw;
$match = [regex]::Match($cs, 'VersionNumberString\s*=\s*"(?<version>(?<major>\d+).(?<minor>\d+).(?<build>\d+)).\d+"\s*;');
$cs = $cs.Replace($match.Value, ('VersionNumberString = "{0}.{1}.{2}.0";' -f ($match.Groups["major"].Value),($match.Groups["minor"].Value),(1+[int]::Parse($match.Groups["build"].Value)).ToString())).Trim();
$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False;
[System.IO.File]::WriteAllLines($filename, $cs, $Utf8NoBomEncoding);
Write-Host ("Updated {0} from {1}" -f $filename, ($match.Groups["version"].Value));