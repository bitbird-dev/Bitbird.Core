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
                
        $vars = @{};
        $vars["ProjectTypeFramework"] = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";
        $vars["ProjectTypeStandard"] = "9A19103F-16F7-4668-BE54-9A1E7A4F7556";
        $vars["ProjectTypeFolder"] = "2150E333-8FDC-42A3-9474-1A3956D46DE8";
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
            HandleSolution -Solution $solution -Config $config -Refs $refs
        }

        foreach($project in $config.projects) { 
            HandleProject -Project $project -Config $config -Refs $refs
        }
    }
    catch {
        Write-Error $_.Exception
    }
}
function HandleSolution {
    param(
        [string] $Solution,
        $Config,
        [Hashtable] $Refs
    )
    Write-Host "handling solution $($solution)"
    
    [string] $content = Get-Content $solution -Raw
    
    $guidPattern = '([-A-Z0-9]+)'
    $namePattern = '([-a-zA-Z0-9._]+)'
    $pathPattern = '([-a-zA-Z0-9._\\]+)'
    $projectPattern = '[pP]roject\s*[(]\s*"{TYPE}"\s*[)]\s*=\s*"NAME"\s*,\s*"PATH"\s*,\s*"{PROJECT}"\s*[eE]nd[pP]roject\s*'
    $folderGuid = "2150E333-8FDC-42A3-9474-1A3956D46DE8";
    $configPattern = '\s+{PROJECT}[.].*';
    $nestedPattern = '\s+{PROJECT}\s*=\s*.*';
    $nestedSectionStartPattern = '[gG]lobalSection\s*[(]\s*[nN]ested[pP]rojects\s*[)]\s*=\s*pre[sS]olution';
    $globalConfigPattern = '[gG]lobal[sS]ection\s*[(]\s*[sS]olution[cC]onfiguration[pP]latforms\s*[)]\s*=\s*pre[sS]olution(\s*([-a-zA-Z._| ()]+)\s*=\s([-a-zA-Z._| ()]+))+';
    $configSectionStartPattern = '[gG]lobalSection\s*[(]\s*[pP]roject[cC]onfiguration[pP]latforms\s*[)]\s*=\s*post[sS]olution';

    $match = [regex]::Match($content, 'MinimumVisualStudioVersion\s*=\s*[0-9.]+\s*');
    if (-not $match.Success){
        throw [System.Exception] "Solution file is ill-formatted, cannot find MinimumVisualStudioVersion-entry"
    }
    $projectInsertIndex = $match.Index + $match.Length;

    $rootGuid = $null;
    $rootName = $null;
    $matches = [regex]::Matches($content, $projectPattern.Replace("TYPE",$folderGuid).Replace("NAME",$namePattern).Replace("PATH",$namePattern).Replace("PROJECT", $guidPattern));
    foreach ($match in $matches) {
        $projectName = $match.Groups[1].Value
        $projectGuid = $match.Groups[3].Value;
        
        $matches = [regex]::Matches($content, $nestedPattern.Replace("PROJECT", $projectGuid));
        if ($matches.Count -eq 0){
            $rootGuid = $projectGuid;
            $rootName = $projectName;
            break;
        }
    }
    if ($rootGuid -eq $null){
        throw [System.Exception] "Solution file is ill-formatted, cannot find root folder"
    }
    Write-Host "  found root folder: $($rootName)"
    Write-Host ""

    $slnConfigs = @{};
    $match = [regex]::Match($content, $globalConfigPattern);
    if (-not $match.Success){
        throw [System.Exception] "Solution file is ill-formatted, cannot find solution configurations"
    }
    for ($i=0; $i -lt $match.Groups[2].Captures.Count; $i++){
        $slnConfigs[$match.Groups[2].Captures[$i].Value.Trim()] = $match.Groups[3].Captures[$i].Value.Trim();
    }
    Write-Host -NoNewline "  found $($slnConfigs.Count) solution configs"
    if ($slnConfigs.Count -ne 0){
        Write-Host ":";

        foreach ($slnConfig in $slnConfigs.Values) {
            Write-Host "    $($slnConfig)";
        }
    } else {
        Write-Host "";
    }
    Write-Host "";
    
    Write-Host "  handling folder .External"
    $externalGuid = [Guid]::NewGuid().ToString().ToUpper();
    $match = [regex]::Match($content, $projectPattern.Replace("TYPE",$folderGuid).Replace("NAME",".External").Replace("PATH",".External").Replace("PROJECT", $guidPattern));
    if (-not $match.Success){
        Write-Host "    no project found, adding new"
        $insertContent = "Project(`"{TYPE}`") = `"NAME`", `"PATH`", `"{PROJECT}`"`r`nEndProject`r`n".Replace("TYPE", $folderGuid).Replace("NAME", ".External").Replace("PATH", ".External").Replace("PROJECT", $externalGuid);
        $content = $content.Insert($projectInsertIndex, $insertContent);
        $projectInsertIndex = $projectInsertIndex + $insertContent.Length;        
    }
    else {
        Write-Host "    project found, keeping existing"
        $externalGuid = $match.Groups[1].Value;
    }
                
    $matches = [regex]::Matches($content, $nestedPattern.Replace("PROJECT", $externalGuid));
    if ($matches.Count -ne 0){
        Write-Host "    found $($matches.Count) existing structure entries, removing all, adding new"
        $content = $content | RemoveAllMatches -Matches $matches;
    }
    else{
        Write-Host "    no existing structure entires found"
    }
    Write-Host ""


    $groupGuids = @{};
    foreach ($ref in $Refs.Values){
        if (-not $groupGuids.ContainsKey($ref.group)){
            Write-Host "  handling group $($ref.group)"

            $groupGuid = [Guid]::NewGuid().ToString().ToUpper();            
            $match = [regex]::Match($content, $projectPattern.Replace("TYPE",$folderGuid).Replace("NAME", $ref.group).Replace("PATH", $ref.group).Replace("PROJECT", $guidPattern));
            if (-not $match.Success){
                Write-Host "    no project found, adding new"

                $insertContent = "Project(`"{TYPE}`") = `"NAME`", `"PATH`", `"{PROJECT}`"`r`nEndProject`r`n".Replace("TYPE", $folderGuid).Replace("NAME", $ref.group).Replace("PATH", $ref.group).Replace("PROJECT", $groupGuid);
                $content = $content.Insert($projectInsertIndex, $insertContent);
                $projectInsertIndex = $projectInsertIndex + $insertContent.Length;        
            }
            else {
                Write-Host "    project found, keeping existing"

                $groupGuid = $match.Groups[1].Value;
            }
            $groupGuids[$ref.group] = $groupGuid;
                
            $matches = [regex]::Matches($content, $nestedPattern.Replace("PROJECT", $groupGuid));
            if ($matches.Count -ne 0){
                Write-Host "    found $($matches.Count) existing structure entries, removing all, adding new"
                $content = $content | RemoveAllMatches -Matches $matches;
            }
            else{
                Write-Host "    no existing structure entires found, adding new"
            }

            
            $match = [regex]::Match($content, $nestedSectionStartPattern);
            if (-not $match.Success){
                throw [System.Exception] "Solution file is ill-formatted, cannot find GlobalSection(NestedProjects) = preSolution"
            }
        
            $insertContent = "`r`n`t`t{PROJECT} = {PARENT}".Replace("PROJECT", $groupGuid).Replace("PARENT", $externalGuid)
            $content = $content.Insert($match.Index + $match.Length, $insertContent); 
        }
    }
    Write-Host ""

    foreach ($ref in $Refs.Values){
        Write-Host "  handling ref $($ref.name)"

        $match = [regex]::Match($content, $projectPattern.Replace("TYPE",$guidPattern).Replace("NAME",$namePattern).Replace("PATH",$pathPattern).Replace("PROJECT",($ref.local.project.ToUpper() | ReplaceVariables -Config $Config)));
        
        if (-not $match.Success){
            Write-Host -NoNewline "    no existing ref"
        }
        else {
            Write-Host -NoNewline "    existing ref removed"
            $content = $content | RemoveMatch -Match $match;
        }

        if ($ref.isLocal){
            Write-Host ", adding new";

            $insertContent = "Project(`"{TYPE}`") = `"NAME`", `"PATH`", `"{PROJECT}`"`r`nEndProject`r`n".Replace("TYPE", ($ref.local.projectType | ReplaceVariables -Config $Config)).Replace("NAME", ($ref.local.name | ReplaceVariables -Config $Config)).Replace("PATH", ($ref.local.include | ReplaceVariables -Config $Config)).Replace("PROJECT", ($ref.local.project.ToUpper() | ReplaceVariables -Config $Config))

            Write-Host $insertContent

            $content = $content.Insert($projectInsertIndex, $insertContent);
            $projectInsertIndex = $projectInsertIndex + $insertContent.Length;
        }
        else {
            Write-Host "";
        }
                
        $matches = [regex]::Matches($content, $nestedPattern.Replace("PROJECT", ($ref.local.project.ToUpper() | ReplaceVariables -Config $Config)));
        if ($matches.Count -ne 0){
            Write-Host "    found $($matches.Count) existing structure entries, removing all, adding new"
            $content = $content | RemoveAllMatches -Matches $matches;
        }
        else{
            Write-Host "    no existing structure entires found, adding new"
        }


        $match = [regex]::Match($content, $nestedSectionStartPattern);
        if (-not $match.Success){
            throw [System.Exception] "Solution file is ill-formatted, cannot find GlobalSection(NestedProjects) = preSolution"
        }
        
        $insertContent = "`r`n`t`t{PROJECT} = {PARENT}".Replace("PROJECT", ($ref.local.project.ToUpper() | ReplaceVariables -Config $Config)).Replace("PARENT", ($groupGuids[$ref.group]))
        $content = $content.Insert($match.Index + $match.Length, $insertContent);  
        
                
        $matches = [regex]::Matches($content, $configPattern.Replace("PROJECT", ($ref.local.project.ToUpper() | ReplaceVariables -Config $Config)));
        if ($matches.Count -ne 0){
            Write-Host "    found $($matches.Count) existing config entries, removing all, adding new"
            $content = $content | RemoveAllMatches -Matches $matches;
        }
        else{
            Write-Host "    no existing structure entires found, adding new"
        }

            
        $match = [regex]::Match($content, $configSectionStartPattern);
        if (-not $match.Success){
            throw [System.Exception] "Solution file is ill-formatted, cannot find GlobalSection(ProjectConfigurationPlatforms) = preSolution"
        }

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

            $insertContent = "`r`n`t`t{PROJECT}.SLNCONFIG.ActiveCfg = PROJCONFIG`r`n`t`t{PROJECT}.SLNCONFIG.Build.0 = PROJCONFIG".Replace("PROJECT", ($ref.local.project.ToUpper() | ReplaceVariables -Config $Config)).Replace("SLNCONFIG", $slnConfig).Replace("PROJCONFIG", $projectConfig)
            $content = $content.Insert($match.Index + $match.Length, $insertContent);      
        }
    }

    $content = $content.Trim();
    $content = $content.Replace("`r`n", "`n").Replace("`r", "").Replace("`n", "`r`n");

    $content | Out-File $solution -Encoding utf8 -NoNewline
}

function HandleProject {
    param(
        $Project,
        $Config,
        [Hashtable] $Refs
    )
    $projectFolderDeepLevel = $Project.folder.Split("\").Count;
    $projectFolderReverse = ".";
    for ($i=0; $i -lt $projectFolderDeepLevel; $i++) {
        $projectFolderReverse = "$($projectFolderReverse)\..";
    }

    $projectFile = [System.IO.Path]::Combine($Project.folder, $Project.projectFile);
    $packagesFile = [System.IO.Path]::Combine($Project.folder, $Project.packagesFile);
    Write-Host "handling Project $($projectFile)"
    
    $projectFileContent = Get-Content $projectFile -Raw
    $packagesFileContent = Get-Content $packagesFile -Raw
    
    $match = [regex]::Match($packagesFileContent, "[<]\s*packages\s*[>]");
    if (-not $match.Success){
        throw [System.Exception] "Package file is ill-formatted, cannot find <packages>-tag"
    }    
    $packageFileInsertIndex = $match.Index+$match.Length;
    
    $match = [regex]::Match($projectFileContent, "[<]\s*[iI]tem[gG]roup\s*[>]");
    if (-not $match.Success){
        throw [System.Exception] "Project file is ill-formatted, cannot find <ItemGroup>-tag"
    }    
    
    $projectFileContent = $projectFileContent.Insert($match.Index, "<ItemGroup>`r`n  </ItemGroup>`r`n  ");
    $projectFileInsertIndex = $match.Index + "<ItemGroup>".Length;

    $packagesFileSearchPattern = '[<]package\s*id\s*=\s*"PROJECT"\s*version\s*=\s*"[-0-9a-zA-Z_.]+"\s*targetFramework\s*=\s*"[-0-9a-zA-Z_.]+"\s*[/][>]\s*';
    $projectFileNugetSearchPattern = '[<]\s*[rR]eference\s*[iI]nclude\s*=\s*"PROJECT.*\s*.*\s*[<][/]\s*[rR]eference\s*[>]\s*';
    $projectFileNugetIdSearchPattern = 'PROJECT.*';
    $projectFileLocalSearchPattern = '\s*[<]\s*[pP]roject[rR]eference\s+.*\s*[<][pP]roject.*\s*[<]\s*[nN]ame\s*[>]PROJECT[<][/]\s*[nN]ame\s*[>]\s*[<][/]\s*[pP]roject[rR]eference\s*[>]\s*';
    foreach($refName in $project.usedRefs) {
        Write-Host "  handling ref $($refName)"

        $ref = $Refs[$refName];
        $match = [regex]::Match(($ref.nuget.package | ReplaceVariables -Config $Config), $packagesFileSearchPattern.Replace("PROJECT", "([-0-9a-zA-Z_.]+)"));
        if (-not $match.Success){
            throw [System.Exception] "$(($ref.nuget.package | ReplaceVariables -Config $Config)) is ill-formatted."
        }
        
        $matches = [regex]::Matches($packagesFileContent, $packagesFileSearchPattern.Replace("PROJECT", $match.Groups[1].Value));
        if ($matches.Count -eq 0){
            Write-Host -NoNewline "    $($Project.packagesFile): no existing ref"
        }
        else {
            Write-Host -NoNewline "    $($Project.packagesFile): existing refs ($($matches.Count)) removed"
            $packagesFileContent = $packagesFileContent | RemoveAllMatches -Matches $matches;
        }
        if (-not $ref.isLocal) {
            Write-Host ", adding"

            $insertText = "`r`n  $(($ref.nuget.package | ReplaceVariables -Config $Config))";

            $packagesFileContent = $packagesFileContent.Insert($packageFileInsertIndex, $insertText);
            $packageFileInsertIndex = $packageFileInsertIndex + $insertText.Length;
        }
        else {
            Write-Host ""
        }

        
        

        $match = [regex]::Match(($ref.nuget.include | ReplaceVariables -Config $Config), $projectFileNugetIdSearchPattern.Replace("PROJECT", "([-0-9a-zA-Z_.]+)"));
        if (-not $match.Success){
            throw [System.Exception] "$(($ref.nuget.include | ReplaceVariables -Config $Config)) is ill-formatted."
        }        
        
        $matches = [regex]::Matches($projectFileContent, $projectFileNugetSearchPattern.Replace("PROJECT", $match.Groups[1].Value));
        if ($matches.Count -eq 0){
            Write-Host -NoNewline "    $($Project.projectFile): no existing nuget ref"
        }
        else {
            Write-Host -NoNewline "    $($Project.projectFile): existing nuget refs ($($matches.Count)) removed"
            $projectFileContent = $projectFileContent | RemoveAllMatches -Matches $matches;
        }
        if (-not $ref.isLocal) {
            Write-Host ", adding new"

            $insertText = "`r`n    <Reference Include=""INCLUDE"">`r`n      <HintPath>HINTPATH</HintPath>`r`n    </Reference>".Replace("INCLUDE", ($ref.nuget.include | ReplaceVariables -Config $Config)).Replace("HINTPATH", [System.IO.Path]::Combine($projectFolderReverse,($ref.nuget.hintPath | ReplaceVariables -Config $Config)));

            $projectFileContent = $projectFileContent.Insert($projectFileInsertIndex, $insertText);
            $projectFileInsertIndex = $projectFileInsertIndex + $insertText.Length;
        }
        else {
            Write-Host ""
        }

        

        
        $matches = [regex]::Matches($projectFileContent, $projectFileLocalSearchPattern.Replace("PROJECT", ($ref.local.name | ReplaceVariables -Config $Config)));
        if ($matches.Count -eq 0){
            Write-Host -NoNewline "    $($Project.projectFile): no existing local ref"
        }
        else {
            Write-Host -NoNewline "    $($Project.projectFile): existing local refs ($($matches.Count)) removed"
            $projectFileContent = $projectFileContent | RemoveAllMatches -Matches $matches;
        }
        if ($ref.isLocal) {
            Write-Host ", adding new"

            $insertText = "`r`n    <ProjectReference Include=""INCLUDE"">`r`n      <Project>{PROJECT}</Project>`r`n      <Name>NAME</Name>`r`n    </ProjectReference>".Replace("INCLUDE", [System.IO.Path]::Combine($projectFolderReverse,($ref.local.include | ReplaceVariables -Config $Config))).Replace("PROJECT", ($ref.local.project | ReplaceVariables -Config $Config)).Replace("NAME", ($ref.local.name | ReplaceVariables -Config $Config));

            $projectFileContent = $projectFileContent.Insert($projectFileInsertIndex, $insertText);
            $projectFileInsertIndex = $projectFileInsertIndex + $insertText.Length;
        }
        else {
            Write-Host ""
        }
    }

    $projectFileCleanupEmptyItemGroupsPattern = '\s*[<]\s*[iI]tem[gG]roup\s*[>]\s*[<][/]\s*[iI]tem[gG]roup\s*[>]';

    $projectFileContent = [regex]::Replace($projectFileContent, $projectFileCleanupEmptyItemGroupsPattern, "");
    
    $projectFileContent = $projectFileContent.Trim();
    $packagesFileContent = $packagesFileContent.Trim();

    $projectFileContent | Out-File $projectFile -Encoding utf8 -NoNewline
    $packagesFileContent | Out-File $packagesFile -Encoding utf8 -NoNewline
}

function RemoveAllMatches {
    [cmdletbinding()]
    param(
        [parameter(ValueFromPipeline)]
        [string] $Content,
        $Matches
    )

    for ($i = $Matches.Count-1; $i -ge 0; $i--) {
        $match = $Matches[$i];
        $Content = $Content.Remove($match.Index, $match.Length);
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

function GetRefGroups
{
	$config = Get-Content -Raw -Path "update-project-refs.json" | ConvertFrom-Json
	return $config.refs | %{ $_.group } | sort-object -unique
}

Export-ModuleMember -Function Update-Project-Refs

Register-TabExpansion Update-Project-Refs @{
    Local = { GetRefGroups }
}