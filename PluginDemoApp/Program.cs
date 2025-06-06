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

        // Add plugins from the Plugins assembly
        var pluginsAssemblyPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "PluginDemo.Plugins.dll");

        if (File.Exists(pluginsAssemblyPath))
        {
          catalog.Catalogs.Add(new AssemblyCatalog(pluginsAssemblyPath));
          Console.WriteLine($"Added plugins from: {pluginsAssemblyPath}");
        }
        else
        {
          Console.WriteLine($"Warning: Plugin assembly not found at {pluginsAssemblyPath}");
        }

        // Add plugins from the Plugins directory
        string pluginsDir = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "Plugins");

        if (Directory.Exists(pluginsDir))
        {
          catalog.Catalogs.Add(new DirectoryCatalog(pluginsDir));
          Console.WriteLine($"Added plugins from directory: {pluginsDir}");
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
