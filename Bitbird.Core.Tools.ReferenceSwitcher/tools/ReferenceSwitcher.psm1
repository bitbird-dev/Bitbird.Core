#####################################
#             CONSTANTS
#####################################

[string] $script:guidSlnFramework = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";
[string] $script:guidSlnStandard = "9A19103F-16F7-4668-BE54-9A1E7A4F7556";
[string] $script:guidSlnFolder = "2150E333-8FDC-42A3-9474-1A3956D46DE8";
[string] $script:patternConfig = '([-a-zA-Z0-9_| ]+)';
[string] $script:patternGuid = '([-A-Z0-9]+)';
[string] $script:patternName = '([-a-zA-Z0-9._]+)';
[string] $script:patternTargetFramework = '([-a-zA-Z0-9.]+)';
[string] $script:patternPath = '([-a-zA-Z0-9.:_\\]+)';
[string] $script:patternVersion = '([-a-zA-Z0-9.]+)';
[string] $script:patternSlnProjectFind = '[pP]roject\s*[(]\s*"{TYPE}"\s*[)]\s*=\s*"NAME"\s*,\s*"PATH"\s*,\s*"{PROJECT}"\s*';
[string] $script:patternSlnProjectNonFolder = '[pP]roject\s*[(]\s*"{TYPE}"\s*[)]\s*=\s*"NAME"\s*,\s*"PATH"\s*,\s*"{PROJECT}"\s*[eE]nd[pP]roject\s*';
[string] $script:slnProjectTemplate = "Project(`"{TYPE}`") = `"NAME`", `"PATH`", `"{PROJECT}`"`r`nEndProject`r`n";
[string] $script:patternProjectInsertPosition = 'MinimumVisualStudioVersion\s*=\s*[0-9.]+\s*';
[string] $script:patternSlnProjectNesting = '{PROJECT}\s*=\s{PARENT}\s*';
[string] $script:slnProjectNestingTemplate = "`r`n`t`t{PROJECT} = {PARENT}";
[string] $script:patternSlnProjectConfig = '\s+{PROJECT}[.]SLNCONFIG[.]\S*\s*=\s*PROJETCONFIG';
[string] $script:slnProjectConfigTemplate = "`r`n`t`t{PROJECT}.SLNCONFIG.ActiveCfg = PROJCONFIG`r`n`t`t{PROJECT}.SLNCONFIG.Build.0 = PROJCONFIG";
[string] $script:patternSlnConfig = '\s+PLATFORMCFG\s*=\s*SLNCFG';
[string] $script:patternPackagesEntry = '[<]package\s*id\s*=\s*"PROJECT"\s*version\s*=\s*"VERSION"\s*targetFramework\s*=\s*"TARGETFRAMEWORK"\s*[/][>]\s*';
[string] $script:templatePackagesEntry = "<package id=""PROJECT"" version=""VERSION"" targetFramework=""TARGETFRAMEWORK"" />`r`n  ";
[string] $script:patternPackagesInsertPosition = '[<]\s*packages\s*[>]\s*';
[string] $script:patternProjectReferenceItemGroupPosition = '<ItemGroup>(\s*<Reference\s*Include)';
[string] $script:templateProjectReference = "`r`n    <Reference Include=""INCLUDE"">`r`n      <HintPath>HINTPATH</HintPath>`r`n    </Reference>";
[string] $script:templateProjectItemGroup = "`r`n  <ItemGroup>`r`n  </ItemGroup>";
[int] $script:templateProjectItemGroupInsertIndex = "`r`n  <ItemGroup>".Length;
[string] $script:patternProjectPropertyGroupEnd = '<[/]PropertyGroup>(?!\s*<PropertyGroup)';
[string] $script:patternProjectReference = '[<]\s*[rR]eference\s*[iI]nclude\s*=\s*"NAME,.*\s*.*\s*[<][/]\s*[rR]eference\s*[>]\s*';
[string] $script:patternProjectProjectReference = '[<]\s*[pP]roject[rR]eference\s+.*\s*[<][pP]roject.*\s*[<]\s*[nN]ame\s*[>]NAME[<][/]\s*[nN]ame\s*[>]\s*[<][/]\s*[pP]roject[rR]eference\s*[>]\s*';
[string] $script:templateProjectProjectReference = "`r`n    <ProjectReference Include=""INCLUDE"">`r`n      <Project>{PROJECT}</Project>`r`n      <Name>NAME</Name>`r`n    </ProjectReference>";
[string] $script:patternProjectProjectReferenceItemGroupPosition = '<ItemGroup>(\s*<ProjectReference\s*Include)';
[string] $script:patternProjectEmptyItemGroup = '\s*[<]\s*[iI]tem[gG]roup\s*[>]\s*[<][/]\s*[iI]tem[gG]roup\s*[>]';
[hashtable] $script:patternsSlnSections = @{
	NestedProjects = @{
		Start = 'GlobalSection[(]NestedProjects[)] = preSolution';
		End = 'EndGlobalSection'
	};
	ProjectConfigPlatforms = @{
		Start = 'GlobalSection[(]ProjectConfigurationPlatforms[)] = postSolution';
		End = 'EndGlobalSection'
	};
	SlnConfigPlatforms = @{
		Start = 'GlobalSection[(]SolutionConfigurationPlatforms[)] = preSolution';
		End = 'EndGlobalSection'
	};
};



#####################################
#             SOLUTION
#####################################

function ReadSln {
    param(
		[parameter(Position = 0)]
        [string] $SlnContent
    )
	
	#find all projects
    [string] $pattern = $script:patternSlnProjectFind | Substitute @{
		TYPE = $script:patternGuid;
		NAME = $script:patternName;
		PATH = $script:patternPath;
		PROJECT = $script:patternGuid;
	};	
	$projects = @{};	
	foreach ($match in [regex]::Matches($SlnContent, $pattern)) { 
		$projects[$match.Groups[4].Value] = new-object psobject -Property @{
			ProjectTypeGuid = $match.Groups[1].Value
			Name 			= $match.Groups[2].Value
			Path 			= $match.Groups[3].Value
			Guid 			= $match.Groups[4].Value
		}
	};
	
	#find all nestings
	$nestings = @{};		
	$pattern = $script:patternSlnProjectNesting | Substitute @{
		PROJECT = $script:patternGuid;
		PARENT = $script:patternGuid;
	};
	foreach ($match in [regex]::Matches(($SlnContent | Get-SlnSection "NestedProjects"), $pattern)) { 
		$nestings[$match.Groups[1].Value] = new-object psobject -Property @{
			Parent  = $match.Groups[2].Value
		}
	};
		
	#update nesting fullpath
	foreach ($nesting in $nestings.Values) {
		$path = New-Object System.Collections.Generic.List[System.Object]
		$namePath = New-Object System.Collections.Generic.List[System.Object]
		
		$node = $nesting;
		do {
			$path.Insert(0, $node.Parent);
			$namePath.Insert(0, $projects[$node.Parent].Name);
			
			$node = $nestings[$node.Parent];
		}
		while ($node);
		
		$nesting | Add-Member Path     $path.ToArray()
		$nesting | Add-Member NamePath $namePath.ToArray()
	}
		
	#set project nesting paths
	foreach ($project in $projects.Values) { 
		if ($nestings.ContainsKey($project.Guid)) {
			$nesting = $nestings[$project.Guid];
			$project | Add-Member Nesting $nesting
		}
		else {
			$project | Add-Member Nesting (new-object psobject -Property @{
				Parent = ""
				Path = ""
				NamePath = ""
			});
		}
	}
	
	#find all solution configs
	$configs = @{};		
	$pattern = $script:patternSlnConfig | Substitute @{
		PLATFORMCFG = $script:patternConfig;
		SLNCFG = $script:patternConfig;
	};
	foreach ($match in [regex]::Matches(($SlnContent | Get-SlnSection "SlnConfigPlatforms"), $pattern)) { 
		$configs[$match.Groups[1].Value] = new-object psobject -Property @{
			SlnConfig = $match.Groups[2].Value
		}
	};
	
	return new-object psobject -Property @{
			Projects = $projects
			Configs = $configs
		};
}
function Get-SlnSectionContentIndex {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
		[string] $Content,
		[parameter(Position = 0)]
		[string] $sectionName
    )
	
	$section = $script:patternsSlnSections[$sectionName];
	if (!$section) {
        throw [System.Exception] "Sln-GetSection: Could not find section patterns for section ${$sectionName}."
	}	

	$match = [regex]::Match($Content, $section["Start"]);
	if (-not $match.Success) {
        throw [System.Exception] "Sln-GetSection: Could not find start of section ${$sectionName}."
	}	
	
	return $match.Index + $match.Length;
}
function Get-SlnSection {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
		[string] $Content,
		[parameter(Position = 0)]
		[string] $sectionName
    )
	
	$section = $script:patternsSlnSections[$sectionName];
	if (!$section) {
        throw [System.Exception] "Sln-GetSection: Could not find section patterns for section ${$sectionName}."
	}	

	$match = [regex]::Match($Content, $section["Start"]);
	if (-not $match.Success) {
        throw [System.Exception] "Sln-GetSection: Could not find start of section ${$sectionName}."
	}	
	[int] $startIdx = $match.Index + $match.Length;

	[int] $endIdx = -1;
	foreach ($match in [regex]::Matches($Content, $section["End"])){
		if ($match.Index -gt $startIdx) {
			$endIdx = $match.Index - 1;
			break;
		}
	}
	if ($endIdx -eq -1) {
        throw [System.Exception] "Sln-GetSection: Could not find end of section ${$sectionName}."
	}

    return $Content.Substring($startIdx, $endIdx - $startIdx);
}
function Add-SlnProject {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
		[string] $Content,
		[string] $Type,
		[string] $Name,
		[string] $Path,
		[string] $Project,
		[string] $Nesting = $null,
		[hashtable] $Configs = $null
    )
	
    $match = [regex]::Match($Content, $script:patternProjectInsertPosition);
    if (-not $match.Success){
        throw [System.Exception] "Add-SlnProject: Solution file is ill-formatted, cannot find project insert position"
    }
    [int] $insertIndex = $match.Index + $match.Length;
	
	[string] $insertContent = $script:slnProjectTemplate | Substitute @{
		TYPE = $Type;
		NAME = $Name;
		PATH = $Path;
		PROJECT = $Project;
	};	
	$Content = $Content.Insert($insertIndex, $insertContent);
	
	
	if ($Nesting) {
		$insertIndex = $Content | Get-SlnSectionContentIndex "NestedProjects";
		$insertContent = $script:slnProjectNestingTemplate | Substitute @{
			PROJECT = $Project;
			PARENT = $Nesting;
		};
		$Content = $Content.Insert($insertIndex, $insertContent);
	}		
	
	if ($Configs) {
		$insertIndex = $Content | Get-SlnSectionContentIndex "ProjectConfigPlatforms";
		
		foreach ($config in $Configs.GetEnumerator()) {
			$insertContent = $script:slnProjectConfigTemplate | Substitute @{
				PROJECT = $Project;
				SLNCONFIG = $config.Name;
				PROJCONFIG = $config.Value;
			};
			$Content = $Content.Insert($insertIndex, $insertContent);		
		}   
	}
	
	return $Content;
}
function Remove-SlnProject {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
		[string] $Content,
		[parameter(Position = 0)]
		[string] $Project
    )
	
	[string] $pattern = $script:patternSlnProjectNonFolder | Substitute @{
		TYPE = $script:patternGuid;
		NAME = $script:patternName;
		PATH = $script:patternPath;
		PROJECT = $Project;
	};
	$Content = $Content | RemoveAllMatches ([regex]::Matches($Content, $pattern));
	
	
	$pattern = $script:patternSlnProjectNesting | Substitute @{
		PROJECT = $Project;
		PARENT = $script:patternGuid;
	};
	$Content = $Content | RemoveAllMatches ([regex]::Matches(($Content | Get-SlnSection "NestedProjects"), $pattern));
	
	
	$pattern = $script:patternSlnProjectConfig | Substitute @{
		PROJECT = $Project;
		SLNCONFIG = $script:patternConfig;
		PROJECTCONFIG = $script:patternConfig;
	};
	$Content = $Content | RemoveAllMatches ([regex]::Matches(($Content | Get-SlnSection "ProjectConfigPlatforms"), $pattern));
	
	return $Content;
}
function RewriteSln {
    param(
        [string] $Solution,
		[Hashtable] $Variables,
        $Config,
        [Hashtable] $Refs
    )
    Write-Host "handling solution $($Solution)"
    
    [string] $content = Get-Content $Solution -Raw;
	$sln = ReadSln $content;

	# .External
	Write-Host "  adding folder .External";
	$externalProject = $sln.Projects.Values | Where-Object { $_.Name -eq ".External" -and $_.Nesting.Parent -eq "" } | Select -First 1;
	if ($externalProject) {
		Write-Host "    existing project found";
		[string] $externalGuid = $externalProject.Guid;
	}
	else {
		[string] $externalGuid = [Guid]::NewGuid().ToString().ToUpper();
		$content = $content | Add-SlnProject -Type $Variables["ProjectTypeFolder"] -Name ".External" -Path ".External" -Project $externalGuid;
	}
    Write-Host "";

	# Groups
    $groupGuids = @{};
    foreach ($ref in $Refs.Values){
        if ($groupGuids.ContainsKey($ref.group)){
			continue;
		}
		
		Write-Host "  adding group $($ref.group)";
		
		$groupProject = $sln.Projects.Values | Where-Object { $_.Name -eq $ref.group -and $_.Nesting.Parent -eq $externalGuid } | Select -First 1;
		
		if ($groupProject) {
			Write-Host "    existing project found";
			[string] $groupGuid = $groupProject.Guid;			
		}
		else {			
			[string] $groupGuid = [Guid]::NewGuid().ToString().ToUpper();
			$content = $content | Add-SlnProject -Type $Variables["ProjectTypeFolder"] -Name $ref.group -Path $ref.group -Project $groupGuid -Nesting $externalGuid;
		}
		$groupGuids[$ref.group] = $groupGuid; 
    }
    Write-Host "";

	#Refs
    foreach ($ref in $Refs.Values){
		if ($ref.isLocal){
			Write-Host "  adding ref $($ref.name)";
		}
		else {
			Write-Host "  removing ref $($ref.name)";
		}
		
		[string] $refGuid = $ref.local.project.ToUpper() | ReplaceVariables -Config $Config;
		[string] $groupGuid = $groupGuids[$ref.group];
		
		$refProject = $sln.Projects.Values | Where-Object { $_.Guid -eq $refGuid  -and $_.Nesting.Parent -eq $groupGuid } | Select -First 1;
        
        if ($refProject) {
			if ($ref.isLocal) {
				Write-Host "    existing project found";
			}
			else {
				$content = $content | Remove-SlnProject $refGuid;
			}
			
			continue;
        }
		
		if ($ref.isLocal){
			[string] $projectType = $ref.local.projectType | ReplaceVariables -Config $Config;
			[string] $projectName = $ref.local.name | ReplaceVariables -Config $Config;
			[string] $projectPath = $ref.local.include | ReplaceVariables -Config $Config;
			
			$projectConfigs = @{};
			foreach ($slnConfig in $slnConfigs.Values) {
				$projectConfig = $null;
				foreach ($projConfigMapping in $ref.local.configs) {
					if ($projConfigMapping.sln -eq $slnConfig) {
						[string] $projectConfig = $projConfigMapping.project;
						break;
					}
				}
				if ($projectConfig -eq $null){
					throw [System.Exception] "Cannot find mapping for reference $($ref.name) to solution config $($slnConfig)"
				}
				
				$projectConfigs[$slnConfig] = $projectConfig; 
			}
			
			$content = $content | Add-SlnProject -Type $projectType -Name $projectName -Path $projectPath -Project $refGuid -Nesting $groupGuid -Configs $projectConfigs;
		}
    }
	Write-Host "";

    $content = $content.Trim();
    $content = $content.Replace("`r`n", "`n").Replace("`r", "").Replace("`n", "`r`n");
	
	return @{
		$Solution = $content;
	};
    #$content | Out-File $Solution -Encoding utf8 -NoNewline
}



#####################################
#             PROJECTS
#####################################

function Add-PackagesEntry {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string]$Content,
		[string]$Project,
		[string]$Version,
		[string]$TargetFramework
    )
		
    $match = [regex]::Match($content, $script:patternPackagesInsertPosition);
    if (-not $match.Success){
        throw [System.Exception] "Add-PackagesEntry: Packages file is ill-formatted, cannot find project insert position"
    }
    [int] $insertIndex = $match.Index + $match.Length;
	
	[string] $insertContent = $script:templatePackagesEntry | Substitute @{
		PROJECT = $Project;
		VERSION = $Version;
		TARGETFRAMEWORK = $TargetFramework;
	};
	$Content = $Content.Insert($insertIndex, $insertContent);
	
	return $Content;
}
function Remove-PackagesEntry {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string]$Content,
        [parameter(Position = 0)]
		[string]$Project
    )

	[string] $pattern = $script:patternPackagesEntry | Substitute @{
		PROJECT = $Project;
		VERSION = $script:patternVersion;
		TARGETFRAMEWORK = $script:patternTargetFramework;
	};
	
	$Content = $Content | RemoveAllMatches ([regex]::Matches($Content, $pattern));
	
	return $Content;
}
function RewritePackagesFile {
    param(
        $Project,
        $Config,
        [Hashtable] $Refs,
		[string] $ProjectFolderReverse
    )
	
	Write-Host "  updating packages.config"
	
    [string] $filePath = [System.IO.Path]::Combine($Project.folder, "packages.config");    
    [string] $content = Get-Content $filePath -Raw
	
	foreach($usedRef in ($project.usedRefs | Reverse)) {
        if ($ref.isLocal) {
			Write-Host "    removing ref $($usedRef)"
        }
		else {
			Write-Host "    adding ref $($usedRef)"
		}
		
        $ref = $Refs[$usedRef];

		[string] $refName            = $ref.nuget.name | ReplaceVariables -Config $Config;
		[string] $refVersion         = $ref.nuget.version | ReplaceVariables -Config $Config;
		[string] $refTargetFramework = $ref.nuget.targetFramework | ReplaceVariables -Config $Config;

		$content = $content | Remove-PackagesEntry $refName;
				
        if (-not $ref.isLocal) {
			$content = $content | Add-PackagesEntry -Project $refName -Version $refVersion -TargetFramework $refTargetFramework
        }
    }

    $content = $content.Trim();
	
	return @{
		$filePath = $content;
	};
    #$content | Out-File $filePath -Encoding utf8 -NoNewline
}

function Add-ProjectItemGroup {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string] $Content
    )
	
	$matches = [regex]::Matches($Content, $script:patternProjectPropertyGroupEnd);
	if ($matches.Count -eq 0){
		throw [System.Exception] "Add-ProjectItemGroup: Project file is ill-formatted, cannot find <PropertyGroup>-end-tag"
	} 
	[int] $insertPosition = $matches[0].Index + $matches[0].Length;

	[string] $insertContent = $script:templateProjectItemGroup;
	$Content = $Content.insert($insertPosition, $insertContent);	
	$insertPosition += $script:templateProjectItemGroupInsertIndex;
	
	return new-object psobject -Property @{
			Content = $Content
			InsertPosition = $insertPosition
		};
}
function Remove-ProjectEmptyItemGroups {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string] $Content
    )

    return [regex]::Replace($content, $script:patternProjectEmptyItemGroup, "");    	
}
function Add-ProjectReference {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string] $Content,
		[string] $Include,
		[string] $HintPath
    )
	
	$match = [regex]::Match($Content, $script:patternProjectReferenceItemGroupPosition);
    if ($match.Success){
		[int] $insertPosition = $match.Groups[1].Index;
    } 
	else {	
		$result = $Content | Add-ProjectItemGroup;
		$Content = $result.Content;
		[int] $insertPosition = $result.InsertPosition;
	}
	
	$insertContent = $script:templateProjectReference | Substitute @{
		INCLUDE = $Include;
		HINTPATH = $HintPath;
	};
	return $Content.insert($insertPosition, $insertContent);	
}
function Remove-ProjectReference {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string] $Content,
		[parameter(Position = 0)]
		[string] $Name
    )
	
	[string] $pattern = $script:patternProjectReference | Substitute @{
		NAME = $Name;
	};
	
	return $Content | RemoveAllMatches ([regex]::Matches($Content, $pattern));
}
function Add-ProjectProjectReference {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string] $Content,
		[string] $Name,
		[string] $Project,
		[string] $Include
    )
	
	$match = [regex]::Match($Content, $script:patternProjectProjectReferenceItemGroupPosition);
    if ($match.Success){
		[int] $insertPosition = $match.Groups[1].Index;
    } 
	else {
		$result = $Content | Add-ProjectItemGroup;
		$Content = $result.Content;
		[int] $insertPosition = $result.InsertPosition;
	}
	
	[string] $insertContent = $script:templateProjectProjectReference | Substitute @{
		NAME = $Name;
		PROJECT = $Project;
		INCLUDE = $Include;
	};
	return $Content.insert($insertPosition, $insertContent);
}
function Remove-ProjectProjectReference {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string] $Content,
		[parameter(Position = 0)]
		[string] $Name
    )
	
	[string] $pattern = $script:patternProjectProjectReference | Substitute @{
		NAME = $Name;
	};
	
	return $Content | RemoveAllMatches ([regex]::Matches($Content, $pattern));
}
function RewriteProjectFile {
    param(
        $Project,
        $Config,
        [Hashtable] $Refs,
		[string] $ProjectFolderReverse
    )
	
	Write-Host "  updating $($Project.projectFile)"

    [string] $filePath = [System.IO.Path]::Combine($Project.folder, $Project.projectFile);    
    [string] $content = Get-Content $filePath -Raw
    
    foreach($usedRef in ($project.usedRefs | Reverse)) {
        Write-Host "    handling ref $($usedRef)"

        $ref = $Refs[$usedRef];
		
		[string] $refLocalName = $ref.local.name | ReplaceVariables -Config $Config;
		[string] $refNugetName = $ref.nuget.name | ReplaceVariables -Config $Config;
				
		$content = $content | Remove-ProjectReference $refNugetName;
		$content = $content | Remove-ProjectProjectReference $refLocalName;	
		
		if ($ref.isLocal) {
			[string] $project  = $ref.local.project | ReplaceVariables -Config $Config;
			[string] $name     = $ref.local.name | ReplaceVariables -Config $Config;
			[string] $include  = $ref.local.include | ReplaceVariables -Config $Config;
			
			$include  = [System.IO.Path]::Combine($projectFolderReverse,$include);
			if ($include.StartsWith(".\") -or $include.StartsWith("./")){
				$include = $include.Substring(2);
			}
			
			$content = $content | Add-ProjectProjectReference -Include $include -Name $name -Project $project;
		}
		else {
			[string] $include  = $ref.nuget.include | ReplaceVariables -Config $Config;
			[string] $hintPath = $ref.nuget.hintPath | ReplaceVariables -Config $Config;
			
			$hintPath  = [System.IO.Path]::Combine($projectFolderReverse,$hintPath);
			if ($hintPath.StartsWith(".\") -or $hintPath.StartsWith("./")){
				$hintPath = $hintPath.Substring(2);
			}
			
			$content = $content | Add-ProjectReference -Include $include -HintPath $hintPath;
		}
    }

    $content = $content | Remove-ProjectEmptyItemGroups;
    $content = $content.Trim();

	return @{
		$filePath = $content;
	};
    #$content | Out-File $filePath -Encoding utf8 -NoNewline
}

function RewriteProject {
    param(
        $Project,
        $Config,
        [Hashtable] $Refs
    )
    Write-Host "handling project $($Project.projectFile) ($($Project.folder))"
	
    [int] $projectFolderDeepLevel = $Project.folder.Split("\").Count;
    [string] $projectFolderReverse = ".";
    for ($i=0; $i -lt $projectFolderDeepLevel; $i++) {
        $projectFolderReverse = "$($projectFolderReverse)\..";
    }
	
	$resuls = @{};
	$results = $results | Merge-Hashtables (RewritePackagesFile -Project $Project -Config $Config -Refs $Refs -ProjectFolderReverse $projectFolderReverse);
	$results = $results | Merge-Hashtables (RewriteProjectFile -Project $Project -Config $Config -Refs $Refs -ProjectFolderReverse $projectFolderReverse);
	return $results;
}



#####################################
#             Helpers
#####################################

function RemoveAllMatches {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string] $Content,
		[parameter(Position = 0)]
        $Matches
    )

	foreach ($match in $Matches) {
        $Content = $Content.Replace($match.Value, "");
    }

    return $Content;
}
function RemoveMatch {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string] $Content,
        $Match
    )

    $Content = $Content.Remove($Match.Index, $Match.Length);

    return $Content;
}
function ReplaceVariables {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string] $Content,
        $Config
    )

    foreach ($var in $Config.variables.Keys){
        $Content = $Content.Replace("{$($var)}", $Config.variables[$var]);
    }

    return $Content;
}
function Substitute {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
		[string]    $Content,
		[parameter(Position = 0)]
		[Hashtable] $Substitutions
    )

    foreach ($substitution in $substitutions.GetEnumerator()){
        $Content = $Content.Replace($substitution.Name, $substitution.Value);
    }

    return $Content;
}
Function Merge-Hashtables {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
		[Hashtable] $Left,
		[parameter(Position = 0)]
		[Hashtable] $Right
    )

	[Hashtable] $result = @{};

	foreach ($kvp in $Left.GetEnumerator()) {
		$result[$kvp.Name] = $kvp.Value;
	}
	foreach ($kvp in $Right.GetEnumerator()) {
		$result[$kvp.Name] = $kvp.Value;
	}

	return $result;
}
function Reverse {
	$arr = @($input);
	[array]::reverse($arr);
	return $arr;
}


#####################################
#             AutoComplete
#####################################

function GetRefGroups {
	$config = Get-Content -Raw -Path "update-project-refs.json" | ConvertFrom-Json
	return $config.refs | %{ $_.group } | sort-object -unique
}



#####################################
#             EXPORTS
#####################################

function Update-Project-Refs {
    param(
        [string] $Local = ""
        )

    try {
        $config = Get-Content -Raw -Path "update-project-refs.json" | ConvertFrom-Json
                
        $localGroups = @{};
        foreach($localGroup in $Local.Split(',')){
            $localGroups[$localGroup.Trim()] = $true;
        }

        $localRefs = New-Object System.Collections.Generic.List[System.Object];
        $nugetRefs = New-Object System.Collections.Generic.List[System.Object];
        $refs = @{};
        foreach($ref in $config.refs) {
            $ref | Add-Member isLocal $localGroups.ContainsKey($ref.group);
            $refs[$ref.name] = $ref;

            if ($ref.isLocal){
                [int] $id = $localRefs.Add($ref);
            }
            else {
                [int] $id = $nugetRefs.Add($ref);
            }
        }

        Write-Host -NoNewline "found $($localRefs.Count) local refs"
        if ($localRefs.Count -ne 0){
            Write-Host ": ";
            foreach ($ref in $localRefs) {
                Write-Host "  $($ref.name)"
            }
        }
        else {
            Write-Host ""
        }
        Write-Host "";

        Write-Host -NoNewline "found $($nugetRefs.Count) nuget refs"
        if ($nugetRefs.Count -ne 0){
            Write-Host ": ";
            foreach ($ref in $nugetRefs) {
                Write-Host "  $($ref.name)"
            }
        }
        else {
            Write-Host ""
        }
        Write-Host "";
        Write-Host "";
                
        $vars = @{
			ProjectTypeFramework = $script:guidSlnFramework;
			ProjectTypeStandard = $script:guidSlnStandard
			ProjectTypeFolder = $script:guidSlnFolder;
		};
        foreach ($var in $Config.variables){
            $vars[$var.name] = $var.value;
        }
		
		
        $Config.variables = $vars;
        Write-Host -NoNewline "found $($vars.Count) variables"
        if ($vars.Count -ne 0){
            Write-Host ": ";
            foreach ($var in $vars.Keys) {
                Write-Host "  $($var) = $($vars[$var])"
            }
        }
        else {
            Write-Host ""
        }
        Write-Host "";

		$results = @{};
        foreach($solution in $config.solutions) {
            $results = $results | Merge-Hashtables (RewriteSln -Solution $solution -Variables $vars -Config $config -Refs $refs);
        }
        foreach($project in $config.projects) { 
            $results = $results | Merge-Hashtables (RewriteProject -Project $project -Config $config -Refs $refs);
        }

		Write-Host "`r`nwriting results to disc...";
		foreach ($entry in $results.GetEnumerator()){
			$entry.Value | Out-File $entry.Name -Encoding utf8 -NoNewline
		}
		Write-Host "  done.";

		Write-Host "`r`n`r`nATTENTION:`r`n  If Visual Studio shows a dialog with the option to 'Discard'`r`n  (i.e. discard Visual Studio's changes and reload the .sln-file'),`r`n  you had unsaved changes in the *.sln-file.`r`n  You might want to save them before running this command again.`r`n`r`n  If there is a dialog where the only possibiltiy is to 'Reload', DO it.`r`n  This is the case if you didn't have unsaved changes.`r`n`r`n  If there is no dialog at all, Visual Studio did not notice the changes in the *.sln-file.`r`n  In this case you need to Close and Reopen the solution.`r`n  This might happen for the solution only!`r`n  In this case Visual Studio reloads the projects but not the solution.";
    }
    catch {
        Write-Error $_.Exception
    }
}
Export-ModuleMember -Function Update-Project-Refs
Register-TabExpansion Update-Project-Refs @{
    Local = { GetRefGroups }
}