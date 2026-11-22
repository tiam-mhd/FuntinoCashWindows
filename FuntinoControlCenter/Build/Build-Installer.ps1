[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$buildRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $buildRoot
$versionFile = Join-Path $buildRoot 'Version.props'
$versionXml = [xml](Get-Content -LiteralPath $versionFile -Raw)
$productVersion = [string]$versionXml.Project.PropertyGroup.FuntinoProductVersion
if ($productVersion -notmatch '^\d+\.\d+\.\d+$') {
    throw "Build\Version.props must contain a semantic version such as 1.2.3. Found: '$productVersion'."
}

$prerequisiteRoot = Join-Path $buildRoot 'Prerequisites'
$packages = @(
    [pscustomobject]@{
        TargetOS = 'Windows7x86'
        TargetLabel = 'Windows7-x86'
        TargetArchitectures = 'x86'
        Prerequisites = @(
            'DotNet461\NDP461-KB3102436-x86-x64-AllOS-ENU.exe',
            'Sql2014\SQLEXPR_x86_ENU.exe',
            'Windows7\Windows6.1-KB4490628-x86.msu',
            'Windows7\Windows6.1-KB4474419-v3-x86.msu'
        )
    },
    [pscustomobject]@{
        TargetOS = 'Windows7x64'
        TargetLabel = 'Windows7-x64'
        TargetArchitectures = 'x64os'
        Prerequisites = @(
            'DotNet461\NDP461-KB3102436-x86-x64-AllOS-ENU.exe',
            'Sql2014\SQLEXPR_x64_ENU.exe',
            'Windows7\Windows6.1-KB4490628-x64.msu',
            'Windows7\Windows6.1-KB4474419-v3-x64.msu'
        )
    },
    [pscustomobject]@{
        TargetOS = 'Windows8'
        TargetLabel = 'Windows8-x64'
        TargetArchitectures = 'x64os'
        Prerequisites = @(
            'DotNet461\NDP461-KB3102436-x86-x64-AllOS-ENU.exe',
            'Sql2017\SQLEXPR_x64_ENU.exe'
        )
    },
    [pscustomobject]@{
        TargetOS = 'Windows81'
        TargetLabel = 'Windows8.1-x64'
        TargetArchitectures = 'x64os'
        Prerequisites = @(
            'DotNet461\NDP461-KB3102436-x86-x64-AllOS-ENU.exe',
            'Sql2017\SQLEXPR_x64_ENU.exe',
            'Windows81\Windows8.1-KB2919442-x64.msu',
            'Windows81\Windows8.1-KB2919355-x64.msu'
        )
    },
    [pscustomobject]@{
        TargetOS = 'Windows10'
        TargetLabel = 'Windows10-x64'
        TargetArchitectures = 'x64os'
        Prerequisites = @(
            'DotNet461\NDP461-KB3102436-x86-x64-AllOS-ENU.exe',
            'Sql2019\SQLEXPR_x64_ENU.exe'
        )
    },
    [pscustomobject]@{
        TargetOS = 'Windows11'
        TargetLabel = 'Windows11-x64'
        TargetArchitectures = 'x64os'
        Prerequisites = @(
            'Sql2019\SQLEXPR_x64_ENU.exe'
        )
    }
)

$windowsUpdateHashes = @{
    'Windows6.1-KB4490628-x86.msu' = 'B86849FDA570F906012992A033F6342373C00A3B3EC0089780EABB084A9A0182'
    'Windows6.1-KB4490628-x64.msu' = '8075F6D889BCB27BE6F52ED47081675E5BB8A5390F2F5BFE4EC27A2BB70CBF5E'
    'Windows6.1-KB4474419-v3-x86.msu' = '8CF49FC7AC61E0B217859313A96337B149AB41B3307EB0D9529615142EA34C6C'
    'Windows6.1-KB4474419-v3-x64.msu' = '99312DF792B376F02E25607D2EB3355725C47D124D8DA253193195515FE90213'
    'Windows8.1-KB2919355-x64.msu' = 'B0C9ADA530F5EE90BB962AFA9ED26218C582362315E13B1BA97E59767CB7825D'
}

$requiredRelativePaths = @($packages | ForEach-Object { $_.Prerequisites } | Sort-Object -Unique)
$prerequisiteFiles = @($requiredRelativePaths | ForEach-Object { Join-Path $prerequisiteRoot $_ })
$missingFiles = @($prerequisiteFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })

$missingRequirements = New-Object System.Collections.ArrayList
foreach ($file in $missingFiles) { [void]$missingRequirements.Add("Missing prerequisite media: $file") }
$referenceAssemblies = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1\mscorlib.dll'
if (-not (Test-Path -LiteralPath $referenceAssemblies)) {
    [void]$missingRequirements.Add('The .NET Framework 4.6.1 Developer Pack is missing from the build computer.')
}

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = $null
if (Test-Path -LiteralPath $vswhere) {
    $installationPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -property installationPath
    if ($installationPath) {
        $candidate = Join-Path $installationPath 'MSBuild\Current\Bin\MSBuild.exe'
        if (Test-Path -LiteralPath $candidate) { $msbuild = $candidate }
    }
}
if (-not $msbuild) {
    $candidate = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe'
    if (Test-Path -LiteralPath $candidate) { $msbuild = $candidate }
}
if (-not $msbuild) { [void]$missingRequirements.Add('MSBuild was not found. Install Visual Studio or Visual Studio Build Tools.') }

$innoCandidates = @(
    (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
    (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
    (Join-Path $prerequisiteRoot 'Tooling\Inno Setup 6\ISCC.exe')
)
$innoCommand = Get-Command ISCC.exe -ErrorAction SilentlyContinue
$innoCompiler = if ($innoCommand) { $innoCommand.Source } else { $innoCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1 }
if (-not $innoCompiler) { [void]$missingRequirements.Add('Inno Setup 6 compiler (ISCC.exe) was not found.') }

if ($missingRequirements.Count -gt 0) {
    $missingRequirements | ForEach-Object { Write-Host "- $_" -ForegroundColor Red }
    throw 'Install the missing build tools and place the required official media in Build\Prerequisites as documented. No installer was created.'
}

foreach ($file in $prerequisiteFiles) {
    $fileName = [IO.Path]::GetFileName($file)
    if ($windowsUpdateHashes.ContainsKey($fileName)) {
        $actualHash = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash
        if ($actualHash -ine $windowsUpdateHashes[$fileName]) {
            throw "The Microsoft update package hash does not match the published SHA-256 value: $file"
        }
        continue
    }

    if ([IO.Path]::GetExtension($file) -ieq '.msu') { continue }
    $signature = Get-AuthenticodeSignature -LiteralPath $file
    if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'Microsoft') {
        throw "The prerequisite signature is not valid or is not from Microsoft: $file ($($signature.Status))."
    }
}

$outputDirectory = Join-Path $buildRoot 'Output'
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$solution = Join-Path $projectRoot 'FuntinoControlCenter.sln'
& $msbuild $solution /t:Rebuild /p:Configuration=Release '/p:Platform=Any CPU' /verbosity:minimal
if ($LASTEXITCODE -ne 0) { throw "MSBuild failed with exit code $LASTEXITCODE." }

$application = Join-Path $projectRoot 'bin\Release\FuntinoControlCenter.exe'
if (-not (Test-Path -LiteralPath $application -PathType Leaf)) { throw "MSBuild did not produce $application" }
$applicationVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($application).ProductVersion
if ($applicationVersion -notlike "$productVersion*") {
    throw "The application version '$applicationVersion' does not match Build\Version.props ($productVersion)."
}

$setupScript = Join-Path $buildRoot 'Setup.iss'
$packageManifestEntries = New-Object System.Collections.ArrayList
foreach ($package in $packages) {
    $targetOsDefine = '/DTargetOS="' + $package.TargetOS + '"'
    $targetLabelDefine = '/DTargetLabel="' + $package.TargetLabel + '"'
    $targetArchitecturesDefine = '/DTargetArchitectures=' + $package.TargetArchitectures
    & $innoCompiler "/DProductVersion=$productVersion" $targetOsDefine $targetLabelDefine $targetArchitecturesDefine "/O$outputDirectory" $setupScript
    if ($LASTEXITCODE -ne 0) { throw "Inno Setup failed for $($package.TargetLabel) with exit code $LASTEXITCODE." }

    $installerName = "FuntinoControlCenter-Setup-$($package.TargetLabel)-$productVersion.exe"
    $installer = Join-Path $outputDirectory $installerName
    if (-not (Test-Path -LiteralPath $installer -PathType Leaf)) { throw "Inno Setup did not produce $installer" }

    $entry = [ordered]@{
        target = $package.TargetLabel
        installer = $installerName
        sizeBytes = (Get-Item -LiteralPath $installer).Length
        sha256 = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    [void]$packageManifestEntries.Add($entry)
    Write-Host "Installer created: $installer"
    Write-Host "SHA-256: $($entry.sha256)"
}

$manifest = [ordered]@{
    product = 'Funtino Control Center'
    version = $productVersion
    packages = @($packageManifestEntries.ToArray())
    generatedUtc = [DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')
}
$manifestPath = Join-Path $outputDirectory 'installer-manifest.json'
$manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $manifestPath -Encoding UTF8

$legacyInstaller = Join-Path $outputDirectory "FuntinoControlCenter-Setup-$productVersion.exe"
if (Test-Path -LiteralPath $legacyInstaller -PathType Leaf) {
    try {
        Remove-Item -LiteralPath $legacyInstaller -Force
        Write-Host "Removed the superseded combined installer: $legacyInstaller"
    } catch {
        Write-Warning "Could not remove the superseded combined installer: $legacyInstaller ($($_.Exception.Message))"
    }
}

Write-Host "Installer manifest created: $manifestPath"
