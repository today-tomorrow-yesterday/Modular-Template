<#
.SYNOPSIS
    Bootstrap this modular monolith template with your project name.

.DESCRIPTION
    This script renames the template from "ModularTemplate" to your specified project name.
    It updates:
    - Application configuration (appsettings.json)
    - All namespaces and using statements
    - Project and solution file names
    - Folder names
    - Project references

.PARAMETER ProjectName
    Your project name in PascalCase with dot separator (e.g., "Acme.Orders", "MyCompany.Inventory")

.PARAMETER DryRun
    Preview changes without making them

.EXAMPLE
    .\bootstrap.ps1 -ProjectName "Acme.Orders"

.EXAMPLE
    .\bootstrap.ps1 -ProjectName "Acme.Orders" -DryRun
#>

param(
    [Parameter(Mandatory = $true, HelpMessage = "Project name in PascalCase (e.g., 'Acme.Orders' or 'ModularTemplate')")]
    [ValidatePattern('^[A-Z][a-zA-Z0-9]*(\.[A-Z][a-zA-Z0-9]*)?$')]
    [string]$ProjectName,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

# Validate project name format with helpful error message
if ($ProjectName -notmatch '^[A-Z][a-zA-Z0-9]*(\.[A-Z][a-zA-Z0-9]*)?$') {
    Write-Host "ERROR: ProjectName must be in PascalCase." -ForegroundColor Red
    Write-Host "       Examples: 'ModularTemplate', 'Acme.Orders', 'MyCompany.Inventory'" -ForegroundColor Yellow
    exit 1
}

$ErrorActionPreference = "Stop"

# =============================================================================
# Configuration
# =============================================================================

$TemplateName = "ModularTemplate"
$RootPath = Split-Path -Parent $PSScriptRoot  # Go up from scripts/ to ModularTemplate/

# Derive all naming variants from the input
# Handles both "Acme.Orders" and "ModularTemplate" formats

# Helper function to convert PascalCase to kebab-case
function ConvertTo-KebabCase {
    param([string]$Text)
    # Insert hyphen before each capital letter (except first), then lowercase
    # "ModularTemplate" -> "modular-template"
    ($Text -creplace '([A-Z])', '-$1').TrimStart('-').ToLower()
}

# Helper function to convert PascalCase to space-separated words
function ConvertTo-DisplayWords {
    param([string]$Text)
    # Insert space before each capital letter (except first)
    # "ModularTemplate" -> "Modular Template"
    ($Text -creplace '([A-Z])', ' $1').TrimStart(' ')
}

if ($ProjectName -match '\.') {
    # Format: "Acme.Orders" (has dot separator)
    $Names = @{
        PascalCase    = $ProjectName                                          # Acme.Orders
        DisplayName   = ($ProjectName -replace '\.', ' ') + " API"            # Acme Orders API
        ShortName     = ($ProjectName -replace '\.', '-').ToLower()           # acme-orders
        DatabaseName  = ($ProjectName -replace '\.', '').ToLower()            # acmeorders
        LowerDot      = $ProjectName.ToLower()                                # acme.orders
    }
} else {
    # Format: "ModularTemplate" (single PascalCase word)
    $kebab = ConvertTo-KebabCase $ProjectName
    $Names = @{
        PascalCase    = $ProjectName                                          # ModularTemplate
        DisplayName   = (ConvertTo-DisplayWords $ProjectName) + " API"        # Modular Template API
        ShortName     = $kebab                                                # modular-template
        DatabaseName  = $ProjectName.ToLower()                                # modulartemplate
        LowerDot      = $kebab -replace '-', '.'                              # modular.template
    }
}

if ($TemplateName -match '\.') {
    # Template has dot separator (e.g., "Modular.Template")
    $TemplateNames = @{
        PascalCase    = $TemplateName
        DisplayName   = ($TemplateName -replace '\.', ' ') + " API"
        ShortName     = ($TemplateName -replace '\.', '-').ToLower()
        DatabaseName  = ($TemplateName -replace '\.', '').ToLower()
        LowerDot      = $TemplateName.ToLower()
    }
} else {
    # Template is single word (e.g., "ModularTemplate")
    $templateKebab = ConvertTo-KebabCase $TemplateName
    $TemplateNames = @{
        PascalCase    = $TemplateName                                     # ModularTemplate
        DisplayName   = (ConvertTo-DisplayWords $TemplateName) + " API"   # Modular Template API
        ShortName     = $templateKebab                                    # modular-template
        DatabaseName  = $TemplateName.ToLower()                           # modulartemplate
        LowerDot      = $templateKebab -replace '-', '.'                  # modular.template
    }
}

# Files to process for text replacement
$FilePatterns = @("*.cs", "*.csproj", "*.sln", "*.json", "*.md", "*.yml", "*.yaml", "*.ps1", "*.sh", "*.runsettings", "Dockerfile", ".editorconfig")

# Directories to exclude
$ExcludeDirs = @("bin", "obj", ".vs", ".git", "node_modules")

# Files to exclude (don't modify bootstrap script while it's running)
$ExcludeFiles = @("bootstrap.ps1")

# =============================================================================
# Helper Functions
# =============================================================================

function Write-Step {
    param([string]$Message)
    Write-Host "`n$Message" -ForegroundColor Cyan
}

function Write-Action {
    param([string]$Message, [switch]$DryRun)
    $prefix = if ($DryRun) { "[DRY RUN] " } else { "" }
    Write-Host "  $prefix$Message" -ForegroundColor Gray
}

function Write-Success {
    param([string]$Message)
    Write-Host "  $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "  WARNING: $Message" -ForegroundColor Yellow
}

function Test-ShouldExclude {
    param([string]$Path)
    foreach ($dir in $ExcludeDirs) {
        if ($Path -match "\\$dir\\") { return $true }
    }
    $fileName = Split-Path -Leaf $Path
    foreach ($file in $ExcludeFiles) {
        if ($fileName -eq $file) { return $true }
    }
    return $false
}

# =============================================================================
# Main Script
# =============================================================================

Write-Host @"

================================================================================
  Modular Monolith Template Bootstrap
================================================================================

  Template:     $TemplateName
  New Name:     $ProjectName

  Derived Names:
    PascalCase:   $($Names.PascalCase)
    DisplayName:  $($Names.DisplayName)
    ShortName:    $($Names.ShortName)
    DatabaseName: $($Names.DatabaseName)

"@ -ForegroundColor White

if ($DryRun) {
    Write-Host "  MODE: DRY RUN (no changes will be made)" -ForegroundColor Yellow
}

Write-Host "================================================================================" -ForegroundColor White

# -----------------------------------------------------------------------------
# Step 1: Clean build artifacts
# -----------------------------------------------------------------------------
Write-Step "Step 1: Cleaning build artifacts..."

$artifactDirs = Get-ChildItem -Path $RootPath -Recurse -Directory -Include "bin", "obj", ".vs" -ErrorAction SilentlyContinue

if ($artifactDirs) {
    foreach ($dir in $artifactDirs) {
        Write-Action "Removing: $($dir.FullName)" -DryRun:$DryRun
        if (-not $DryRun) {
            Remove-Item -Recurse -Force $dir.FullName -ErrorAction SilentlyContinue
        }
    }
    Write-Success "Cleaned $($artifactDirs.Count) artifact directories"
} else {
    Write-Success "No artifact directories found"
}

# -----------------------------------------------------------------------------
# Step 2: Update appsettings.json Application section
# -----------------------------------------------------------------------------
Write-Step "Step 2: Updating appsettings.json..."

$appSettingsFiles = @(
    "$RootPath\src\API\$TemplateName.Api\appsettings.json",
    "$RootPath\src\API\$TemplateName.Api\appsettings.Development.json"
)

foreach ($settingsFile in $appSettingsFiles) {
    if (Test-Path $settingsFile) {
        Write-Action "Processing: $settingsFile" -DryRun:$DryRun

        if (-not $DryRun) {
            $content = Get-Content $settingsFile -Raw

            # Update Application section values
            $content = $content -replace '"Name":\s*"[^"]*"', "`"Name`": `"$($Names.PascalCase)`""
            $content = $content -replace '"DisplayName":\s*"[^"]*"', "`"DisplayName`": `"$($Names.DisplayName)`""
            $content = $content -replace '"ShortName":\s*"[^"]*"', "`"ShortName`": `"$($Names.ShortName)`""
            $content = $content -replace '"DatabaseName":\s*"[^"]*"', "`"DatabaseName`": `"$($Names.DatabaseName)`""

            # Update database name in connection string
            $content = $content -replace "Database=$($TemplateNames.DatabaseName)", "Database=$($Names.DatabaseName)"
            $content = $content -replace "Database=$($TemplateNames.DatabaseName)_dev", "Database=$($Names.DatabaseName)_dev"

            Set-Content -Path $settingsFile -Value $content -NoNewline
        }
        Write-Success "Updated: $(Split-Path $settingsFile -Leaf)"
    }
}

# -----------------------------------------------------------------------------
# Step 3: Replace text content in source files
# -----------------------------------------------------------------------------
Write-Step "Step 3: Replacing text content in files..."

$replacements = @(
    @{ Old = $TemplateNames.PascalCase; New = $Names.PascalCase },      # ModularTemplate -> Acme.Orders
    @{ Old = $TemplateNames.LowerDot; New = $Names.LowerDot }           # modular.template -> acme.orders
)

$filesUpdated = 0

foreach ($pattern in $FilePatterns) {
    $files = Get-ChildItem -Path $RootPath -Recurse -File -Filter $pattern -ErrorAction SilentlyContinue |
        Where-Object { -not (Test-ShouldExclude $_.FullName) }

    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }

        $modified = $false
        $newContent = $content

        foreach ($replacement in $replacements) {
            if ($newContent -match [regex]::Escape($replacement.Old)) {
                $newContent = $newContent -replace [regex]::Escape($replacement.Old), $replacement.New
                $modified = $true
            }
        }

        if ($modified) {
            Write-Action "Updated: $($file.FullName)" -DryRun:$DryRun
            if (-not $DryRun) {
                Set-Content -Path $file.FullName -Value $newContent -NoNewline
            }
            $filesUpdated++
        }
    }
}

Write-Success "Updated $filesUpdated files"

# -----------------------------------------------------------------------------
# Step 4: Rename files (.csproj, .sln)
# -----------------------------------------------------------------------------
Write-Step "Step 4: Renaming files..."

$filesRenamed = 0

# Get all files that need renaming, sorted by path length descending (deepest first)
$filesToRename = Get-ChildItem -Path $RootPath -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object {
        ($_.Name -like "*$($TemplateNames.PascalCase)*") -and
        (-not (Test-ShouldExclude $_.FullName))
    } |
    Sort-Object { $_.FullName.Length } -Descending

foreach ($file in $filesToRename) {
    $newName = $file.Name -replace [regex]::Escape($TemplateNames.PascalCase), $Names.PascalCase

    if ($newName -ne $file.Name) {
        Write-Action "$($file.Name) -> $newName" -DryRun:$DryRun
        if (-not $DryRun) {
            Rename-Item -Path $file.FullName -NewName $newName
        }
        $filesRenamed++
    }
}

Write-Success "Renamed $filesRenamed files"

# -----------------------------------------------------------------------------
# Step 5: Rename folders
# -----------------------------------------------------------------------------
Write-Step "Step 5: Renaming folders..."

$foldersRenamed = 0

# Must process folders iteratively since renaming changes paths
# Loop until no more folders match
do {
    $folder = Get-ChildItem -Path $RootPath -Recurse -Directory -ErrorAction SilentlyContinue |
        Where-Object {
            ($_.Name -like "*$($TemplateNames.PascalCase)*") -and
            (-not (Test-ShouldExclude $_.FullName))
        } |
        Sort-Object { $_.FullName.Length } -Descending |
        Select-Object -First 1

    if ($folder) {
        $newName = $folder.Name -replace [regex]::Escape($TemplateNames.PascalCase), $Names.PascalCase
        Write-Action "$($folder.Name) -> $newName" -DryRun:$DryRun

        if (-not $DryRun) {
            Rename-Item -Path $folder.FullName -NewName $newName
        }
        $foldersRenamed++
    }
} while ($folder -and (-not $DryRun))

# In dry run mode, just count all folders that would be renamed
if ($DryRun) {
    $foldersRenamed = (Get-ChildItem -Path $RootPath -Recurse -Directory -ErrorAction SilentlyContinue |
        Where-Object {
            ($_.Name -like "*$($TemplateNames.PascalCase)*") -and
            (-not (Test-ShouldExclude $_.FullName))
        }).Count
}

Write-Success "Renamed $foldersRenamed folders"

# -----------------------------------------------------------------------------
# Step 6: Verification
# -----------------------------------------------------------------------------
Write-Step "Step 6: Verification..."

if (-not $DryRun) {
    # Check for any remaining references to old name
    $remaining = Get-ChildItem -Path $RootPath -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object { -not (Test-ShouldExclude $_.FullName) } |
        ForEach-Object {
            $content = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
            if ($content -match [regex]::Escape($TemplateNames.PascalCase)) {
                $_.FullName
            }
        } |
        Where-Object { $_ }

    if ($remaining) {
        Write-Warning "Found remaining references to '$($TemplateNames.PascalCase)' in:"
        $remaining | ForEach-Object { Write-Host "    $_" -ForegroundColor Yellow }
    } else {
        Write-Success "No remaining references to '$($TemplateNames.PascalCase)' found"
    }
}

# -----------------------------------------------------------------------------
# Summary
# -----------------------------------------------------------------------------
Write-Host @"

================================================================================
  Bootstrap Complete!
================================================================================
"@ -ForegroundColor Green

if ($DryRun) {
    Write-Host "  This was a DRY RUN. No changes were made." -ForegroundColor Yellow
    Write-Host "  Run without -DryRun to apply changes." -ForegroundColor Yellow
} else {
    Write-Host @"
  Your project has been renamed to: $ProjectName

  Next steps:
    1. Run: dotnet restore
    2. Run: dotnet build
    3. Run: dotnet test
    4. Update your git remote if needed

  The following values are now derived from Application config:
    - API Title:      $($Names.DisplayName)
    - EventBusName:   $($Names.ShortName)-events
    - EventSource:    $($Names.LowerDot)
    - Auth Audience:  $($Names.ShortName)-api

"@ -ForegroundColor White
}

Write-Host "================================================================================" -ForegroundColor White
