#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs tests with code coverage and generates HTML report.
.PARAMETER OpenReport
    Opens the HTML report in default browser after generation.
.PARAMETER Filter
    Optional test filter expression.
.EXAMPLE
    ./scripts/run-coverage.ps1
    ./scripts/run-coverage.ps1 -OpenReport
    ./scripts/run-coverage.ps1 -Filter "FullyQualifiedName~Domain"
#>

param(
    [switch]$OpenReport,
    [string]$Filter
)

$ErrorActionPreference = "Stop"
$solutionRoot = Split-Path -Parent $PSScriptRoot
$coverageDir = Join-Path $solutionRoot "coverage"
$testResultsDir = Join-Path $solutionRoot "TestResults"
$coberturaFile = Join-Path $coverageDir "coverage.cobertura.xml"

# Clean previous results
if (Test-Path $coverageDir) { Remove-Item -Recurse -Force $coverageDir }
if (Test-Path $testResultsDir) { Remove-Item -Recurse -Force $testResultsDir }
New-Item -ItemType Directory -Path $coverageDir -Force | Out-Null

Write-Host "Running tests with coverage..." -ForegroundColor Cyan

# Build test command
$testArgs = @(
    "test"
    $solutionRoot
    "--collect:Code Coverage"
    "--settings:$solutionRoot\ModularTemplate.runsettings"
    "--results-directory:$testResultsDir"
)

if ($Filter) {
    $testArgs += "--filter:$Filter"
}

# Run tests
& dotnet @testArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Find all coverage files (.coverage binary format)
$coverageFiles = Get-ChildItem -Path $testResultsDir -Recurse -Filter "*.coverage" | Select-Object -ExpandProperty FullName

if ($coverageFiles.Count -eq 0) {
    Write-Host "No coverage files found!" -ForegroundColor Red
    exit 1
}

Write-Host "`nFound $($coverageFiles.Count) coverage file(s)" -ForegroundColor Green
Write-Host "Merging and converting to Cobertura format..." -ForegroundColor Cyan

# Merge coverage files and convert to Cobertura using dotnet-coverage
& dotnet dotnet-coverage merge $coverageFiles --output $coberturaFile --output-format cobertura

Write-Host "Generating HTML report..." -ForegroundColor Cyan

# Generate HTML report
& dotnet reportgenerator `
    "-reports:$coberturaFile" `
    "-targetdir:$coverageDir" `
    "-reporttypes:Html;TextSummary" `
    "-title:ModularTemplate Coverage Report"

# Display summary (first 30 lines)
$summaryFile = Join-Path $coverageDir "Summary.txt"
if (Test-Path $summaryFile) {
    Write-Host "`n--- Coverage Summary ---" -ForegroundColor Yellow
    Get-Content $summaryFile | Select-Object -First 30
}

# Open report if requested
if ($OpenReport) {
    $indexFile = Join-Path $coverageDir "index.html"
    if (Test-Path $indexFile) {
        Start-Process $indexFile
    }
}

Write-Host "`nReport generated at: $coverageDir\index.html" -ForegroundColor Green
