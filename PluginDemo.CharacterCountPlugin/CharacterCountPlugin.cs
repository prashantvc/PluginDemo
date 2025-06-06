using System.ComponentModel.Composition;
using System.Linq; // Added for Count() extension method
using PluginDemo.Shared;

namespace PluginDemo.CharacterCountPlugin
{
  /// <summary>
  /// A sample plugin that counts characters in the input
  /// </summary>
  [Export(typeof(IPlugin))]
  public class CharacterCountPlugin : PluginBase
  {
    /// <summary>
    /// Gets the name of the plugin
    /// </summary>
    public override string Name => "Character Counter";

    /// <summary>
    /// Gets the version of the plugin
    /// </summary>
    public override string Version => "1.0.2";

    /// <summary>
    /// Executes the plugin operation by counting characters in the input
    /// </summary>
    /// <param name="input">The input string to count characters in</param>
    /// <returns>A summary of character count statistics</returns>
    public override string Execute(string input)
    {
      if (string.IsNullOrEmpty(input))
        return "Input is empty";

      int totalChars = input.Length;
      int letters = input.Count(c => char.IsLetter(c));
      int digits = input.Count(c => char.IsDigit(c));
      int spaces = input.Count(c => char.IsWhiteSpace(c));
      int others = totalChars - letters - digits - spaces;

      return $"Character Analysis:\n" +
             $"Total characters: {totalChars}\n" +
             $"Letters: {letters}\n" +
             $"Digits: {digits}\n" +
             $"Spaces: {spaces}\n" +
             $"Other characters: {others}";
    }
  }
}
