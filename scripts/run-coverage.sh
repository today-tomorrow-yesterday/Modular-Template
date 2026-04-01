#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_ROOT="$(dirname "$SCRIPT_DIR")"
COVERAGE_DIR="$SOLUTION_ROOT/coverage"
TEST_RESULTS_DIR="$SOLUTION_ROOT/TestResults"
COBERTURA_FILE="$COVERAGE_DIR/coverage.cobertura.xml"

# Parse arguments
OPEN_REPORT=false
FILTER=""

while [[ $# -gt 0 ]]; do
    case $1 in
        --open) OPEN_REPORT=true; shift ;;
        --filter) FILTER="$2"; shift 2 ;;
        *) echo "Unknown option: $1"; exit 1 ;;
    esac
done

# Clean previous results
rm -rf "$COVERAGE_DIR" "$TEST_RESULTS_DIR"
mkdir -p "$COVERAGE_DIR"

echo "Running tests with coverage..."

# Build test command
TEST_CMD="dotnet test \"$SOLUTION_ROOT\" --collect:\"Code Coverage\" --settings:\"$SOLUTION_ROOT/ModularTemplate.runsettings\" --results-directory:\"$TEST_RESULTS_DIR\""

if [ -n "$FILTER" ]; then
    TEST_CMD="$TEST_CMD --filter \"$FILTER\""
fi

# Run tests
eval $TEST_CMD

# Find coverage files (.coverage binary format)
COVERAGE_FILES=$(find "$TEST_RESULTS_DIR" -name "*.coverage" -type f)

if [ -z "$COVERAGE_FILES" ]; then
    echo "No coverage files found!"
    exit 1
fi

FILE_COUNT=$(echo "$COVERAGE_FILES" | wc -l)
echo ""
echo "Found $FILE_COUNT coverage file(s)"
echo "Merging and converting to Cobertura format..."

# Merge coverage files and convert to Cobertura
dotnet dotnet-coverage merge $COVERAGE_FILES --output "$COBERTURA_FILE" --output-format cobertura

echo "Generating HTML report..."

# Generate HTML report
dotnet reportgenerator \
    "-reports:$COBERTURA_FILE" \
    "-targetdir:$COVERAGE_DIR" \
    "-reporttypes:Html;TextSummary" \
    "-title:ModularTemplate Coverage Report"

# Display summary (first 30 lines)
if [ -f "$COVERAGE_DIR/Summary.txt" ]; then
    echo ""
    echo "--- Coverage Summary ---"
    head -30 "$COVERAGE_DIR/Summary.txt"
fi

# Open report if requested
if [ "$OPEN_REPORT" = true ]; then
    if command -v xdg-open &> /dev/null; then
        xdg-open "$COVERAGE_DIR/index.html"
    elif command -v open &> /dev/null; then
        open "$COVERAGE_DIR/index.html"
    fi
fi

echo ""
echo "Report generated at: $COVERAGE_DIR/index.html"
