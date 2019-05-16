
function Create-Api-Model-Template {
	[string] $templateMetaData = Get-Content ([System.IO.Path]::Combine($PSScriptRoot, "..", "resources", "ApiModelTemplate.tt")) -Raw;
	$templateMetaData | Out-File "ApiModelTemplate.tt" -Encoding utf8 -NoNewline
}

Export-ModuleMember -Function Create-Api-Model-Template