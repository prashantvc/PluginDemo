// filepath: c:\Users\PrashantCholachagudd\source\repos\PluginDemo\PluginDemoApp\Program.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using PluginDemo.Shared;

namespace PluginDemoApp
{
  class Program
  {
    [ImportMany]
    public IEnumerable<IPlugin> Plugins { get; set; }

    static void Main(string[] args)
    {
      Console.WriteLine("MEF Plugin Demo Application");
      Console.WriteLine("==========================\n");

      var program = new Program();
      program.ComposePlugins();
      program.RunPlugins();

      Console.WriteLine("\nPress any key to exit...");
      Console.ReadKey();
    }

    private void ComposePlugins()
    {
      try
      {
        // Create catalog of parts for plugin discovery
        var catalog = new AggregateCatalog();

        // Application base directory
        string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

        // Dynamically discover and load plugin assemblies
        Console.WriteLine("Searching for plugins...");

        // Directories to search for plugins
        var searchDirectories = new List<string> { baseDir };

        // Optional: Add a dedicated plugins directory if it exists
        var pluginsDir = Path.Combine(baseDir, "Plugins");
        if (Directory.Exists(pluginsDir))
        {
          searchDirectories.Add(pluginsDir);
        }

        int pluginsFound = 0;
        HashSet<string> loadedAssemblies = new HashSet<string>();

        // First remove old Plugins DLL that might cause duplicates
        var oldPluginsPath = Path.Combine(baseDir, "Plugins", "PluginDemo.Plugins.dll");
        if (File.Exists(oldPluginsPath))
        {
          try
          {
            // On Windows, we might not be able to delete in use files, but we can try
            File.Delete(oldPluginsPath);
            Console.WriteLine("Removed old plugins assembly to avoid duplicates");
          }
          catch
          {
            Console.WriteLine("Warning: Could not remove old plugins assembly - you may see duplicate plugins");
          }
        }

        // Search in all directories
        foreach (var directory in searchDirectories)
        {
          // Get all DLL files - we'll check if they contain plugins when loading
          var pluginFiles = Directory.GetFiles(directory, "*.dll")
              .Where(file =>
              {
                var fileName = Path.GetFileName(file).ToLowerInvariant();
                // Exclude the shared library, main app DLL, and system DLLs
                return !fileName.EndsWith("plugindemo.shared.dll") &&
                       !fileName.Equals("plugindemoapp.dll") &&
                       !fileName.StartsWith("system.") &&
                       !fileName.StartsWith("microsoft.");
              });

          if (!pluginFiles.Any())
          {
            Console.WriteLine($"No plugin assemblies found in: {directory}");
            continue;
          }

          foreach (var pluginPath in pluginFiles)
          {
            // Skip already loaded assemblies (by full path to avoid duplicates)
            if (!loadedAssemblies.Add(pluginPath.ToLowerInvariant()))
            {
              Console.WriteLine($"Skipping duplicate: {Path.GetFileName(pluginPath)}");
              continue;
            }

            try
            {
              // Load assembly to check if it contains any IPlugin types before adding to catalog
              var assembly = Assembly.LoadFrom(pluginPath);

              // Check if this assembly contains any types implementing IPlugin
              bool hasPlugins = assembly.GetExportedTypes()
                  .Any(type =>
                      typeof(IPlugin).IsAssignableFrom(type) &&
                      !type.IsAbstract &&
                      type.IsClass);

              if (hasPlugins)
              {
                catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                Console.WriteLine($"Added plugin assembly: {Path.GetFileName(pluginPath)}");
                pluginsFound++;
              }
            }
            catch (ReflectionTypeLoadException)
            {
              // This is normal for non-plugin assemblies, only show for verbose logging
              Console.WriteLine($"Skipping non-plugin assembly: {Path.GetFileName(pluginPath)}");
            }
            catch (BadImageFormatException)
            {
              // This happens with native DLLs and is normal
              // Just skip these files silently
            }
            catch (Exception ex)
            {
              // Show error message with details
              Console.WriteLine($"Error examining {Path.GetFileName(pluginPath)}: {ex.Message}");
            }
          }
        }

        if (pluginsFound == 0)
        {
          Console.WriteLine("Warning: No plugin assemblies were successfully loaded.");
        }

        // Create the CompositionContainer with the parts in the catalog
        var container = new CompositionContainer(catalog);

        // Fill the imports of this object
        container.ComposeParts(this);

        // Remove duplicate plugins by type name
        if (Plugins != null)
        {
          var uniquePlugins = Plugins.GroupBy(p => p.GetType().FullName)
                                    .Select(g => g.First())
                                    .ToList();

          if (uniquePlugins.Count < Plugins.Count())
          {
            Console.WriteLine($"Removed {Plugins.Count() - uniquePlugins.Count} duplicate plugins");
            Plugins = uniquePlugins;
          }
        }

        Console.WriteLine($"\nDiscovered {Plugins?.Count() ?? 0} plugins\n");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error composing plugins: {ex.Message}");
        if (ex.InnerException != null)
        {
          Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
      }
    }

    private void RunPlugins()
    {
      if (Plugins == null || !Plugins.Any())
      {
        Console.WriteLine("No plugins were found.");
        return;
      }

      Console.WriteLine("Available plugins:");

      int index = 1;
      foreach (var plugin in Plugins)
      {
        Console.WriteLine($"{index++}. {plugin.Name} (v{plugin.Version})");
      }

      while (true)
      {
        Console.WriteLine("\nEnter plugin number (or 'q' to quit): ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "q")
          break;

        if (int.TryParse(input, out int pluginNumber) && pluginNumber > 0 && pluginNumber <= Plugins.Count())
        {
          var plugin = Plugins.ElementAt(pluginNumber - 1);
          Console.WriteLine($"\nSelected plugin: {plugin.Name} (v{plugin.Version})");
          Console.WriteLine("Enter text to process (or empty line to cancel): ");
          var textInput = Console.ReadLine();

          if (!string.IsNullOrWhiteSpace(textInput))
          {
            try
            {
              string result = plugin.Execute(textInput);
              Console.WriteLine($"\nResult: {result}");
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Error executing plugin: {ex.Message}");
            }
          }
        }
        else
        {
          Console.WriteLine("Invalid selection.");
        }
      }
    }
  }
}
