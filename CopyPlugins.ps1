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

# Copy plugin DLLs and their dependencies to the Plugins directory
foreach ($project in $pluginProjects) {
    $sourcePath = ".\$project\bin\Debug\net9.0"
    $sourceDll = "$sourcePath\$project.dll"
    
    # Create a subdirectory for each plugin
    $pluginSubDir = Join-Path $pluginsDir $project
    if (-not (Test-Path $pluginSubDir)) {
        New-Item -Path $pluginSubDir -ItemType Directory | Out-Null
        Write-Host "Created plugin directory: $pluginSubDir"
    }
    
    if (Test-Path $sourceDll) {
        # Copy the main plugin DLL to its own subdirectory
        Copy-Item -Path $sourceDll -Destination $pluginSubDir -Force
        Write-Host "Copied $sourceDll to $pluginSubDir"
        
        # Copy all dependency DLLs to the plugin's subdirectory
        $dependencies = Get-ChildItem -Path $sourcePath -Filter "*.dll" | Where-Object { 
            $_.Name -ne "$project.dll" -and 
            -not $_.Name.StartsWith("System.") -and
            -not $_.Name.StartsWith("Microsoft.NETCore.") -and
            -not $_.Name.StartsWith("Microsoft.Win32.") -and
            -not $_.Name.StartsWith("netstandard")
        }
        
        foreach ($dep in $dependencies) {
            Copy-Item -Path $dep.FullName -Destination $pluginSubDir -Force
            Write-Host "Copied dependency $($dep.Name) to $pluginSubDir"
        }
    } else {
        Write-Host "Warning: $sourceDll not found. Build the project first."
    }
}

Write-Host "
Plugins copied successfully. Run the application to use them."
