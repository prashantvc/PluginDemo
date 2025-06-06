# Define plugin projects and the target directory
$pluginProjects = @(
    "PluginDemo.CharacterCountPlugin",
    "PluginDemo.ReverseStringPlugin",
    "PluginDemo.UpperCasePlugin"
)
$pluginsDir = ".\PluginDemoApp\bin\Debug\net9.0\Plugins"

# Create the Plugins directory if it doesn't exist
if (-not (Test-Path $pluginsDir)) {
    New-Item -Path $pluginsDir -ItemType Directory | Out-Null
    Write-Host "Created Plugins directory: $pluginsDir"
}

# Copy plugin DLLs to the Plugins directory
foreach ($project in $pluginProjects) {
    $sourceDll = ".\$project\bin\Debug\net9.0\$project.dll"
    if (Test-Path $sourceDll) {
        Copy-Item -Path $sourceDll -Destination $pluginsDir -Force
        Write-Host "Copied $sourceDll to $pluginsDir"
    } else {
        Write-Host "Warning: $sourceDll not found. Build the project first."
    }
}

Write-Host "
Plugins copied successfully. Run the application to use them."
