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
        // Create catalog of parts from the plugin assembly
        var catalog = new AggregateCatalog();

        // Add each plugin assembly
        string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

        // Add plugins from individual project assemblies
        var pluginAssemblies = new[]
        {
            "PluginDemo.CharacterCountPlugin.dll",
            "PluginDemo.ReverseStringPlugin.dll",
            "PluginDemo.UpperCasePlugin.dll"
        };

        foreach (var pluginAssembly in pluginAssemblies)
        {
          var pluginPath = Path.Combine(baseDir, pluginAssembly);
          if (File.Exists(pluginPath))
          {
            catalog.Catalogs.Add(new AssemblyCatalog(pluginPath));
            Console.WriteLine($"Added plugin from: {pluginPath}");
          }
          else
          {
            Console.WriteLine($"Warning: Plugin assembly not found at {pluginPath}");
          }
        }

        // Create the CompositionContainer with the parts in the catalog
        var container = new CompositionContainer(catalog);

        // Fill the imports of this object
        container.ComposeParts(this);

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
