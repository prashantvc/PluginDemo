#nullable enable

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
    public IEnumerable<IPlugin> Plugins { get; set; } = Array.Empty<IPlugin>();

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
        var catalog = new AggregateCatalog();
        string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        string pluginsDir = Path.Combine(baseDir, "Plugins");

        Console.WriteLine("Searching for plugins...");

        // Create plugins directory if it doesn't exist
        if (!Directory.Exists(pluginsDir))
        {
          Directory.CreateDirectory(pluginsDir);
          Console.WriteLine($"Created plugins directory: {pluginsDir}");
        }

        // Try to clean up legacy plugins to avoid duplicates
        CleanupLegacyPlugins(pluginsDir);

        // Load plugins from the plugins directory
        LoadPluginsFromDirectory(catalog, pluginsDir);

        // Create container and compose parts
        var container = new CompositionContainer(catalog);
        container.ComposeParts(this);

        // Remove duplicates
        RemoveDuplicatePlugins();

        Console.WriteLine($"\nDiscovered {Plugins.Count()} plugins\n");
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

    private void CleanupLegacyPlugins(string pluginsDir)
    {
      var oldPluginsPath = Path.Combine(pluginsDir, "PluginDemo.Plugins.dll");
      if (File.Exists(oldPluginsPath))
      {
        try
        {
          File.Delete(oldPluginsPath);
          Console.WriteLine("Removed old plugins assembly to avoid duplicates");
        }
        catch
        {
          Console.WriteLine("Warning: Could not remove old plugins assembly - you may see duplicate plugins");
        }
      }
    }

    private void LoadPluginsFromDirectory(AggregateCatalog catalog, string directory)
    {
      var pluginFiles = Directory.GetFiles(directory, "PluginDemo.*.dll");

      if (!pluginFiles.Any())
      {
        Console.WriteLine($"No plugin assemblies found in: {directory}");
        return;
      }

      var loadedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      int pluginsFound = 0;

      foreach (var pluginPath in pluginFiles)
      {
        // Skip already loaded assemblies
        if (!loadedAssemblies.Add(pluginPath))
        {
          Console.WriteLine($"Skipping duplicate: {Path.GetFileName(pluginPath)}");
          continue;
        }

        try
        {
          // Load assembly
          var assembly = Assembly.LoadFrom(pluginPath);

          // Add to catalog - no need to filter by name since we're already targeting PluginDemo.*.dll
          catalog.Catalogs.Add(new AssemblyCatalog(assembly));
          Console.WriteLine($"Added plugin assembly: {Path.GetFileName(pluginPath)}");
          pluginsFound++;
        }
        catch (ReflectionTypeLoadException)
        {
          Console.WriteLine($"Skipping non-plugin assembly: {Path.GetFileName(pluginPath)}");
        }
        catch (BadImageFormatException)
        {
          // Skip native DLLs silently
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Error examining {Path.GetFileName(pluginPath)}: {ex.Message}");
        }
      }

      if (pluginsFound == 0)
      {
        Console.WriteLine("Warning: No plugin assemblies were successfully loaded.");
      }
    }

    private void RemoveDuplicatePlugins()
    {
      if (!Plugins.Any()) return;

      var uniquePlugins = Plugins.GroupBy(p => p.GetType().FullName)
                               .Select(g => g.First())
                               .ToList();

      if (uniquePlugins.Count < Plugins.Count())
      {
        Console.WriteLine($"Removed {Plugins.Count() - uniquePlugins.Count} duplicate plugins");
        Plugins = uniquePlugins;
      }
    }

    private void RunPlugins()
    {
      if (!Plugins.Any())
      {
        Console.WriteLine("No plugins were found.");
        return;
      }

      Console.WriteLine("Available plugins:");
      DisplayPluginList();
      ExecutePluginLoop();
    }

    private void DisplayPluginList()
    {
      int index = 1;
      foreach (var plugin in Plugins)
      {
        Console.WriteLine($"{index++}. {plugin.Name} (v{plugin.Version})");
      }
    }

    private void ExecutePluginLoop()
    {
      while (true)
      {
        Console.WriteLine("\nEnter plugin number (or 'q' to quit): ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "q")
          break;

        ExecutePluginIfValid(input);
      }
    }

    private void ExecutePluginIfValid(string input)
    {
      if (!int.TryParse(input, out int pluginNumber) ||
          pluginNumber <= 0 ||
          pluginNumber > Plugins.Count())
      {
        Console.WriteLine("Invalid selection.");
        return;
      }

      var plugin = Plugins.ElementAt(pluginNumber - 1);
      Console.WriteLine($"\nSelected plugin: {plugin.Name} (v{plugin.Version})");
      Console.WriteLine("Enter text to process (or empty line to cancel): ");

      var textInput = Console.ReadLine();
      if (string.IsNullOrWhiteSpace(textInput))
        return;

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
}
