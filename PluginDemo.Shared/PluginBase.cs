using System.ComponentModel.Composition;

namespace PluginDemo.Shared
{
  /// <summary>
  /// Base class for plugins that can be used to provide some common functionality
  /// </summary>
  public abstract class PluginBase : IPlugin
  {
    /// <summary>
    /// Gets the name of the plugin
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the version of the plugin
    /// </summary>
    public virtual string Version => "1.0.0";

    /// <summary>
    /// Executes the plugin operation
    /// </summary>
    /// <param name="input">The input data for the plugin</param>
    /// <returns>The result of the plugin operation</returns>
    public abstract string Execute(string input);
  }
}
