# Restores a binary dependency from a GitHub Release if it's not already present.
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$DependencyName,

    [switch]$Force
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Exit early if no work is needed to keep build times fast.
$ProjectRoot = (Split-Path -Path $PSScriptRoot -Parent)
$DepPath = Join-Path -Path $ProjectRoot -ChildPath $DependencyName
$VersionFile = Join-Path -Path $DepPath -ChildPath "version.workflow.info"

if (-not (Test-Path $VersionFile)) {
    exit 0
}

if (-not $Force) {
    if (Test-Path $DepPath) {
        $ExistingFilesCount = (Get-ChildItem -Path $DepPath | Where-Object { $_.Name -ne 'version.workflow.info' } | Measure-Object).Count
        if ($ExistingFilesCount -gt 0) {
            # Exit silently to avoid noise in build logs on every restore.
            exit 0
        }
    }
}

# Only perform more expensive checks now that we know a download is required.
Write-Host "Action: Restoring '$DependencyName'..."
$DefaultRepo = "iplaylf2/lf2-taiwu-mods"
$RepoIdentifier = $null

# Priority 1: User override via environment variable.
if ($env:LF2_DEPS_REPO) {
    Write-Host "Info: Using repository override from LF2_DEPS_REPO: $($env:LF2_DEPS_REPO)"
    $RepoIdentifier = $env:LF2_DEPS_REPO
}
# Priority 2: CI environment variable.
elseif ($env:GITHUB_REPOSITORY) {
    $RepoIdentifier = $env:GITHUB_REPOSITORY
}
# Priority 3: Hardcoded default for local/forked builds.
else {
    Write-Host "Info: Defaulting to upstream dependency repository: $DefaultRepo"
    $RepoIdentifier = $DefaultRepo
}

try {
    $RepoOwner, $RepoName = $RepoIdentifier.split("/")
    if (-not $RepoOwner -or -not $RepoName) {
        throw "Invalid repository format: '$RepoIdentifier'. Expected 'owner/repo'."
    }
}
catch {
    Write-Error "Fatal: Could not determine GitHub repository from '$RepoIdentifier'. Please ensure the format is 'owner/repo'."
    exit 1
}

if (-not (Test-Path $DepPath)) {
    New-Item -Path $DepPath -ItemType Directory | Out-Null
}

if ($Force) {
    $ExistingFiles = Get-ChildItem -Path $DepPath | Where-Object { $_.Name -ne 'version.workflow.info' }
    if ($ExistingFiles.Length -gt 0) {
        Write-Host "Info: Force mode enabled. Cleaning existing files..."
        $ExistingFiles | Remove-Item -Recurse -Force
    }
}

$DepVersion = (Get-Content -Path $VersionFile).Trim()
$ArtifactName = "$DependencyName.zip"
$DownloadUrl = "https://github.com/$RepoOwner/$RepoName/releases/download/$DepVersion/$ArtifactName"
$DestinationZip = Join-Path -Path $ProjectRoot -ChildPath $ArtifactName

$Headers = @{}
if ($env:GITHUB_TOKEN) {
    $Headers["Authorization"] = "token $env:GITHUB_TOKEN"
}

try {
    Write-Host "Info: Downloading version '$DepVersion'..."
    Invoke-WebRequest -Uri $DownloadUrl -OutFile $DestinationZip -Headers $Headers

    Write-Host "Info: Extracting archive..."
    Expand-Archive -Path $DestinationZip -DestinationPath $DepPath -Force

    Write-Host "Success: Successfully restored '$DependencyName'."
}
catch {
    Write-Error "Fatal: Failed to restore '$DependencyName'. Error: $_"
    exit 1
}
finally {
    if (Test-Path $DestinationZip) {
        Remove-Item -Path $DestinationZip
    }
}
