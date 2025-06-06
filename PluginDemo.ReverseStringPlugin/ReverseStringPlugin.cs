using System;
using System.ComponentModel.Composition;
using PluginDemo.Shared;

namespace PluginDemo.ReverseStringPlugin
{
  /// <summary>
  /// A sample plugin that reverses the input string
  /// </summary>
  [Export(typeof(IPlugin))]
  public class ReverseStringPlugin : PluginBase
  {
    /// <summary>
    /// Gets the name of the plugin
    /// </summary>
    public override string Name => "String Reverser";

    /// <summary>
    /// Gets the version of the plugin
    /// </summary>
    public override string Version => "1.1.0";

    /// <summary>
    /// Executes the plugin operation by reversing the input string
    /// </summary>
    /// <param name="input">The input string to reverse</param>
    /// <returns>The reversed string</returns>
    public override string Execute(string input)
    {
      if (string.IsNullOrEmpty(input))
        return string.Empty;

      char[] charArray = input.ToCharArray();
      Array.Reverse(charArray);
      return new string(charArray);
    }
  }
}
