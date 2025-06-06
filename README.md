# MEF Plugin Demo

This solution demonstrates the use of the Managed Extensibility Framework (MEF) to create a plugin-based application in .NET.

## Project Structure

The solution consists of three projects:

1. **PluginDemoApp**: A console application that hosts and discovers plugins.
2. **PluginDemo.Shared**: A class library containing shared interfaces and base classes for plugins.
3. **PluginDemo.Plugins**: A class library containing sample plugin implementations.

## How It Works

The MEF framework allows for creating extensible applications through the use of:

- **Exports**: Components that provide functionality (plugins)
- **Imports**: Components that consume functionality (the host application)
- **Composition**: The process of connecting exports to imports

In this demo:

- `IPlugin` interface in the Shared project defines the contract that all plugins must implement.
- `PluginBase` abstract class provides a base implementation for plugins.
- The Plugins project contains sample plugins that are exported using the `[Export]` attribute.
- The main application imports plugins using the `[ImportMany]` attribute and composes them using a `CompositionContainer`.

## Running the Demo

1. Build the solution
2. Run the PluginDemoApp console application
3. The application will discover and display available plugins
4. Select a plugin by number and provide input to see the plugin in action

## Extending with New Plugins

To create a new plugin:

1. Create a new class in the PluginDemo.Plugins project or in a separate library
2. Implement the `IPlugin` interface or inherit from `PluginBase`
3. Add the `[Export(typeof(IPlugin))]` attribute to your plugin class
4. Build and place the assembly in the application's directory or a "Plugins" subdirectory

## Plugin Examples Included

1. **Upper Case Converter**: Converts input text to uppercase
2. **String Reverser**: Reverses the characters in the input text
3. **Character Counter**: Analyzes and counts different types of characters in the input

## Benefits of MEF

- Loose coupling between components
- Dynamic discovery of extensions at runtime
- No need to modify host application to add new plugins
- Metadata-driven capabilities through attributes
