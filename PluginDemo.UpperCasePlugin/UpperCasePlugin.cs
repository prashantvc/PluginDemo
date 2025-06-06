using System.ComponentModel.Composition;
using PluginDemo.Shared;

namespace PluginDemo.UpperCasePlugin
{
  /// <summary>
  /// A sample plugin that converts input to uppercase
  /// </summary>
  [Export(typeof(IPlugin))]
  public class UpperCasePlugin : PluginBase
  {
    /// <summary>
    /// Gets the name of the plugin
    /// </summary>
    public override string Name => "Upper Case Converter";

    /// <summary>
    /// Gets the version of the plugin
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Executes the plugin operation by converting input to uppercase
    /// </summary>
    /// <param name="input">The input string to convert</param>
    /// <returns>The uppercase version of the input</returns>
    public override string Execute(string input)
    {
      return input?.ToUpper() ?? string.Empty;
    }
  }
}
