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
[string] $script:patternSlnProjectNesting = '{PROJECT}\s*=\s{PARENT}';
[string] $script:slnProjectNestingTemplate = "`r`n`t`t{PROJECT} = {PARENT}";
[string] $script:patternSlnProjectConfig = '\s+{PROJECT}[.]SLNCONFIG[.]\S*\s*=\s*PROJETCONFIG';
[string] $script:slnProjectConfigTemplate = "`r`n`t`t{PROJECT}.SLNCONFIG.ActiveCfg = PROJCONFIG`r`n`t`t{PROJECT}.SLNCONFIG.Build.0 = PROJCONFIG";
[string] $script:patternSlnConfig = '\s+PLATFORMCFG\s*=\s*SLNCFG';

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
	$startIdx = $match.Index + $match.Length;

	$endIdx = -1;
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
	
	$pattern = $script:patternSlnProjectNonFolder | Substitute @{
		TYPE = $script:patternGuid;
		NAME = $script:patternName;
		PATH = $script:patternPath;
		PROJECT = $Project;
	};
	$Content = $Content | RemoveAllMatches [regex]::Matches($Content, $pattern);
	
	
	$pattern = $script:patternSlnProjectNesting | Substitute @{
		PROJECT = $Project;
		PARENT = $script:patternGuid;
	};
	$Content = $Content | RemoveAllMatches [regex]::Matches(($Content | Get-SlnSection "NestedProjects"), $pattern);
	
	
	$pattern = $script:patternSlnProjectConfig | Substitute @{
		PROJECT = $Project;
		SLNCONFIG = $script:patternConfig;
		PROJECTCONFIG = $script:patternConfig;
	};
	$Content = $Content | RemoveAllMatches [regex]::Matches(($Content | Get-SlnSection "ProjectConfigPlatforms"), $pattern);
	
	return $Content;
}
function RewriteSln {
    param(
        [string] $Solution,
		[Hashtable] $Variables,
        $Config,
        [Hashtable] $Refs
    )
    Write-Host "handling solution $($solution)"
    
    [string] $content = Get-Content $solution -Raw;
	$sln = ReadSln $content;
	
    
    $folderGuid = $Variables["ProjectTypeFolder"];
    $configPattern = '\s+{PROJECT}[.].*';
    $nestedPattern = '\s+{PROJECT}\s*=\s*.*';
    $nestedSectionStartPattern = '[gG]lobalSection\s*[(]\s*[nN]ested[pP]rojects\s*[)]\s*=\s*pre[sS]olution';
    $globalConfigPattern = '[gG]lobal[sS]ection\s*[(]\s*[sS]olution[cC]onfiguration[pP]latforms\s*[)]\s*=\s*pre[sS]olution(\s*([-a-zA-Z._| ()]+)\s*=\s([-a-zA-Z._| ()]+))+';
    $configSectionStartPattern = '[gG]lobalSection\s*[(]\s*[pP]roject[cC]onfiguration[pP]latforms\s*[)]\s*=\s*post[sS]olution';


	# .External
	Write-Host "  adding folder .External";
	$externalProject = $sln.Projects.Values | Where-Object { $_.Name -eq ".External" -and $_.Nesting.Parent -eq "" } | Select -First 1;
	if ($externalProject) {
			Write-Host "    existing project found";
		$externalGuid = $externalProject.Guid;
	}
	else {
		$externalGuid = [Guid]::NewGuid().ToString().ToUpper();
		$content = $content | Add-SlnProject -Type $folderGuid -Name ".External" -Path ".External" -Project $externalGuid;
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
			$groupGuid = $groupProject.Guid;			
		}
		else {			
			$groupGuid = [Guid]::NewGuid().ToString().ToUpper();
			$content = $content | Add-SlnProject -Type $folderGuid -Name $ref.group -Path $ref.group -Project $groupGuid -Nesting $externalGuid;
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
		
		$refGuid = $ref.local.project.ToUpper() | ReplaceVariables -Config $Config;
		$groupGuid = $groupGuids[$ref.group];
		
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
			$projectType = $ref.local.projectType | ReplaceVariables -Config $Config;
			$projectName = $ref.local.name | ReplaceVariables -Config $Config;
			$projectPath = $ref.local.include | ReplaceVariables -Config $Config;
			
			$projectConfigs = @{};
			foreach ($slnConfig in $slnConfigs.Values) {
				$projectConfig = $null;
				foreach ($projConfigMapping in $ref.local.configs) {
					if ($projConfigMapping.sln -eq $slnConfig) {
						$projectConfig = $projConfigMapping.project;
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

    $content | Out-File $solution -Encoding utf8 -NoNewline
}



#####################################
#             PROJECTS
#####################################

[string] $script:patternPackagesEntry = '[<]package\s*id\s*=\s*"PROJECT"\s*version\s*=\s*"VERSION"\s*targetFramework\s*=\s*"TARGETFRAMEWORK"\s*[/][>]\s*';
[string] $script:templatePackagesEntry = '<package id="PROJECT" version="VERSION" targetFramework="TARGETFRAMEWORK" />';
[string] $script:patternPackagesInsertPosition = '[<]\s*packages\s*[>]';
[string] $script:patternProjectReferenceItemGroupPosition = '<ItemGroup>\s*(<Reference\s*Include)';
[string] $script:templateProjectReference = '`r`n    <Reference Include="INCLUDE">`r`n      <HintPath>HINTPATH</HintPath>`r`n    </Reference>';
[string] $script:templateProjectItemGroup = '`r`n  <ItemGroup>`r`n  <ItemGroup>';
[int] $script:templateProjectItemGroupInsertIndex = '`r`n  <ItemGroup>'.Length;
[string] $script:patternProjectPropertyGroupEnd = '<[/]PropertyGroup>';
[string] $script:patternProjectReference = '[<]\s*[rR]eference\s*[iI]nclude\s*=\s*"PROJECTNAME.*\s*.*\s*[<][/]\s*[rR]eference\s*[>]\s*';
[string] $script:patternProjectProjectReference = '[<]\s*[pP]roject[rR]eference\s+.*\s*[<][pP]roject.*\s*[<]\s*[nN]ame\s*[>]NAME[<][/]\s*[nN]ame\s*[>]\s*[<][/]\s*[pP]roject[rR]eference\s*[>]\s*';
[string] $script:templateProjectProjectReference = '`r`n    <ProjectReference Include="INCLUDE">`r`n      <Project>{PROJECT}</Project>`r`n      <Name>NAME</Name>`r`n    </ProjectReference>';
[string] $script:patternProjectProjectReferenceItemGroupPosition = '<ItemGroup>\s*(<ProjectReference\s*Include)';
[string] $script:patternProjectEmptyItemGroup = '\s*[<]\s*[iI]tem[gG]roup\s*[>]\s*[<][/]\s*[iI]tem[gG]roup\s*[>]';

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
	
    $filePath = [System.IO.Path]::Combine($Project.folder, "packages.config");    
    $content = Get-Content $filePath -Raw
	
	foreach($usedRef in $project.usedRefs) {
        if ($ref.isLocal) {
			Write-Host "    removing ref $($usedRef)"
        }
		else {
			Write-Host "    adding ref $($usedRef)"
		}
		
        $ref = $Refs[$usedRef];
		
		$refName            = $ref.nuget.name | ReplaceVariables -Config $Config;
		$refVersion         = $ref.nuget.version | ReplaceVariables -Config $Config;
		$refTargetFramework = $ref.nuget.targetFramework | ReplaceVariables -Config $Config;
				
		$content = $content | Remove-PackagesEntry $refName;
				
        if (-not $ref.isLocal) {
			$content = $content | Add-PackagesEntry -Project $refName -Version $refVersion -TargetFramework $refTargetFramework
        }
    }

    $content = $content.Trim();

    $content | Out-File $filePath -Encoding utf8 -NoNewline
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
	$insertPosition = $matches[$matches.Count - 1].Index + $matches[$matches.Count - 1].Length;

	$insertContent = $script:templateProjectItemGroup;
	$Content = $Content.insert($insertPosition, $insertContent);	
	$insertIndex += $script:templateProjectItemGroupInsertIndex;
	
	return new-object psobject -Property @{
			Content = $Content
			InsertIndex = $insertIndex
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
		$insertPosition = $match.Groups[1].Index;
    } 
	else {	
		$result = $Content | Add-ProjectItemGroup;
		$content = $result.Content;
		$insertPosition = $result.InsertPosition;
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
	
	$pattern = $script:patternProjectReference | Substitute @{
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
	
	$match = [regex]::Match($Content, $script:patternProjectReferenceItemGroupPosition);
    if ($match.Success){
		$insertPosition = $match.Groups[1].Index;
    } 
	else {
		$result = $Content | Add-ProjectItemGroup;
		$Content = $result.Content;
		$insertPosition = $result.InsertPosition;
	}
	
	$insertContent = $script:templateProjectProjectReference | Substitute @{
		NAME = $Include;
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
	
	$pattern = $script:patternProjectProjectReference | Substitute @{
		PROJECTNAME = $Name;
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

    $filePath = [System.IO.Path]::Combine($Project.folder, $Project.projectFile);    
    $content = Get-Content $filePath -Raw
    
    foreach($usedRef in $project.usedRefs) {
        Write-Host "    handling ref $($usedRef)"

        $ref = $Refs[$usedRef];
		
		[string] $refLocalName = $ref.local.name | ReplaceVariables -Config $Config;
		[string] $refNugetName = $ref.nuget.include | ReplaceVariables -Config $Config;
		
		$content = $content | Remove-ProjectReference $refNugetName;
		$content = $content | Remove-ProjectProjectReference $refLocalName;	
		
		if ($ref.isLocal) {
			[string] $project  = $ref.local.project | ReplaceVariables -Config $Config;
			[string] $name     = $ref.local.name | ReplaceVariables -Config $Config;
			[string] $include  = $ref.local.include | ReplaceVariables -Config $Config;
			
			$include  = [System.IO.Path]::Combine($projectFolderReverse,$include);
			
			$content = $content | Add-ProjectProjectReference -Include $include -Name $name -Project $project;
		}
		else {
			[string] $include  = $ref.nuget.include | ReplaceVariables -Config $Config;
			[string] $hintPath = $ref.nuget.hintPath | ReplaceVariables -Config $Config;
			
			$content = $content | Add-ProjectReference -Include $include -HintPath $hintPath;
		}
    }

    $content = $content | Remove-ProjectEmptyItemGroups;
    $content = $content.Trim();

    $content | Out-File $filePath -Encoding utf8 -NoNewline
}

function RewriteProject {
    param(
        $Project,
        $Config,
        [Hashtable] $Refs
    )
    Write-Host "handling project $($Project.projectFile) ($($Project.folder))"
	
    $projectFolderDeepLevel = $Project.folder.Split("\").Count;
    $projectFolderReverse = ".";
    for ($i=0; $i -lt $projectFolderDeepLevel; $i++) {
        $projectFolderReverse = "$($projectFolderReverse)\..";
    }
	
	RewritePackagesFile -Project $Project -Config $Config -Refs $Refs -ProjectFolderReverse $projectFolderReverse;
	RewriteProjectFile -Project $Project -Config $Config -Refs $Refs -ProjectFolderReverse $projectFolderReverse;
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

    #try {
        $config = Get-Content -Raw -Path "update-project-refs.json" | ConvertFrom-Json
                
        $localGroups = @{};
        foreach($localGroup in $Local.Split(',')){
            $localGroups[$localGroup.Trim()] = $true;
        }

        $localRefs = [System.Collections.ArrayList]@();
        $nugetRefs = [System.Collections.ArrayList]@();
        $refs = @{};
        foreach($ref in $config.refs) {
            $ref | Add-Member isLocal $localGroups.ContainsKey($ref.group);
            $refs[$ref.name] = $ref;

            if ($ref.isLocal){
                $id = $localRefs.Add($ref);
            }
            else {
                $id = $nugetRefs.Add($ref);
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

        foreach($solution in $config.solutions) {
            RewriteSln -Solution $solution -Variables $vars -Config $config -Refs $refs
        }

        foreach($project in $config.projects) { 
            RewriteProject -Project $project -Config $config -Refs $refs
        }
    #}
    #catch {
    #    Write-Error $_.Exception
    #}
}
Export-ModuleMember -Function Update-Project-Refs
#Register-TabExpansion Update-Project-Refs @{
#    Local = { GetRefGroups }
#}