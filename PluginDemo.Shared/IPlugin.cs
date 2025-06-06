using System.ComponentModel.Composition;

namespace PluginDemo.Shared
{
  /// <summary>
  /// Interface that all plugins must implement
  /// </summary>
  public interface IPlugin
  {
    /// <summary>
    /// Gets the name of the plugin
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of the plugin
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Executes the plugin operation
    /// </summary>
    /// <param name="input">The input data for the plugin</param>
    /// <returns>The result of the plugin operation</returns>
    string Execute(string input);
  }
}
