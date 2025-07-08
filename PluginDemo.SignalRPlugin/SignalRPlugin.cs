using System.ComponentModel.Composition;
using Microsoft.AspNetCore.SignalR.Client;
using PluginDemo.Shared;

namespace PluginDemo.SignalRPlugin
{
  /// <summary>
  /// A plugin that subscribes to a SignalR hub for status keepalive messages
  /// </summary>
  [Export(typeof(IPlugin))]
  public class SignalRPlugin : PluginBase
  {
    private HubConnection? _connection;
    private readonly string _hubUrl = "http://localhost:5000/status-keepalive-hub";
    private readonly List<string> _receivedMessages = new();
    private bool _isConnected = false;

    /// <summary>
    /// Gets the name of the plugin
    /// </summary>
    public override string Name => "SignalR Hub Subscriber";

    /// <summary>
    /// Gets the version of the plugin
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Executes the plugin operation - automatically connects to SignalR hub and listens for messages
    /// </summary>
    /// <param name="input">Any input triggers the automatic connection and message monitoring</param>
    /// <returns>Status information about the connection and message monitoring</returns>
    public override string Execute(string input)
    {
      // Auto-connect and start monitoring messages
      return ConnectAndMonitorMessages();
    }

    /// <summary>
    /// Connects to the SignalR hub
    /// </summary>
    private string ConnectToHub()
    {
      try
      {
        if (_isConnected)
          return "Already connected to SignalR hub";

        _connection = new HubConnectionBuilder()
          .WithUrl(_hubUrl)
          .WithAutomaticReconnect()
          .Build();

        // Subscribe to hub methods - listen for StatusUpdate messages
        _connection.On<object>("StatusUpdate", (statusData) =>
        {
          var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
          var logMessage = $"[{timestamp}] StatusUpdate: {statusData}";
          _receivedMessages.Add(logMessage);

          // Print message immediately when received
          Console.WriteLine($"📩 {logMessage}");

          // Keep only the last 50 messages to prevent memory issues
          if (_receivedMessages.Count > 50)
            _receivedMessages.RemoveAt(0);
        });

        // Handle connection events
        _connection.Closed += async (error) =>
        {
          _isConnected = false;
          var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
          var logMessage = $"[{timestamp}] Connection closed" + (error != null ? $": {error.Message}" : "");
          _receivedMessages.Add(logMessage);
          Console.WriteLine($"❌ {logMessage}");
          await Task.CompletedTask;
        };

        _connection.Reconnected += async (connectionId) =>
        {
          _isConnected = true;
          var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
          var logMessage = $"[{timestamp}] Reconnected with ID: {connectionId}";
          _receivedMessages.Add(logMessage);
          Console.WriteLine($"✅ {logMessage}");
          await Task.CompletedTask;
        };

        _connection.Reconnecting += async (error) =>
        {
          _isConnected = false;
          var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
          var logMessage = $"[{timestamp}] Reconnecting..." + (error != null ? $" ({error.Message})" : "");
          _receivedMessages.Add(logMessage);
          Console.WriteLine($"🔄 {logMessage}");
          await Task.CompletedTask;
        };

        // Start the connection
        var task = Task.Run(async () => await _connection.StartAsync());
        task.Wait(TimeSpan.FromSeconds(10)); // Wait up to 10 seconds for connection

        if (_connection.State == HubConnectionState.Connected)
        {
          _isConnected = true;
          var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
          var logMessage = $"[{timestamp}] Connected to SignalR hub: {_hubUrl}";
          _receivedMessages.Add(logMessage);
          Console.WriteLine($"✅ {logMessage}");

          // Subscribe to status updates after connecting (like the HTML does)
          try
          {
            var subscribeTask = Task.Run(async () => await _connection.InvokeAsync("SubscribeToStatus"));
            subscribeTask.Wait(TimeSpan.FromSeconds(5));
            var subscribeMessage = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Subscribed to status updates";
            _receivedMessages.Add(subscribeMessage);
            Console.WriteLine($"📝 {subscribeMessage}");
          }
          catch (Exception ex)
          {
            var errorMessage = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Failed to subscribe: {ex.Message}";
            _receivedMessages.Add(errorMessage);
            Console.WriteLine($"❌ {errorMessage}");
          }

          return $"Successfully connected to SignalR hub at {_hubUrl}\n" +
                 $"Connection ID: {_connection.ConnectionId}\n" +
                 $"State: {_connection.State}";
        }
        else
        {
          return $"Failed to connect to SignalR hub. State: {_connection.State}";
        }
      }
      catch (Exception ex)
      {
        _isConnected = false;
        return $"Error connecting to SignalR hub: {ex.Message}";
      }
    }

    /// <summary>
    /// Disconnects from the SignalR hub
    /// </summary>
    private string DisconnectFromHub()
    {
      try
      {
        if (_connection != null && _isConnected)
        {
          var task = Task.Run(async () => await _connection.StopAsync());
          task.Wait(TimeSpan.FromSeconds(5));

          var disposeTask = Task.Run(async () => await _connection.DisposeAsync());
          disposeTask.Wait(TimeSpan.FromSeconds(2));
          _connection = null;
          _isConnected = false;

          var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
          var logMessage = $"[{timestamp}] Disconnected from SignalR hub";
          _receivedMessages.Add(logMessage);

          return "Successfully disconnected from SignalR hub";
        }
        else
        {
          return "Not connected to any SignalR hub";
        }
      }
      catch (Exception ex)
      {
        _isConnected = false;
        return $"Error disconnecting from SignalR hub: {ex.Message}";
      }
    }

    /// <summary>
    /// Connects to SignalR hub and continuously monitors messages until cancelled
    /// </summary>
    /// <returns>Status information about the monitoring session</returns>
    private string ConnectAndMonitorMessages()
    {
      try
      {
        Console.WriteLine("\n=== SignalR Hub Monitor ===");
        Console.WriteLine($"Connecting to: {_hubUrl}");
        Console.WriteLine("Press 'q' and Enter to quit monitoring...\n");

        // Connect to the hub
        var connectResult = ConnectToHub();
        Console.WriteLine(connectResult);

        if (!_isConnected)
        {
          return "Failed to connect to SignalR hub. Monitoring cancelled.";
        }

        // Monitor messages continuously
        var cancellationToken = new CancellationTokenSource();

        // Start background task to check for user input
        var inputTask = Task.Run(() =>
        {
          while (!cancellationToken.Token.IsCancellationRequested)
          {
            var key = Console.ReadLine();
            if (key?.ToLower() == "q")
            {
              cancellationToken.Cancel();
              break;
            }
          }
        });

        // Simple monitoring loop - messages are printed immediately in the subscription callback
        while (!cancellationToken.Token.IsCancellationRequested)
        {
          // Small delay to prevent CPU spinning
          Thread.Sleep(500);

          // Check connection status
          if (_connection?.State != HubConnectionState.Connected && _isConnected)
          {
            Console.WriteLine("⚠️  Connection lost. Attempting to reconnect...");
            _isConnected = false;
          }
        }

        // Cleanup
        Console.WriteLine("\n🛑 Monitoring stopped by user.");
        DisconnectFromHub();

        return $"Monitoring session completed. Total messages received: {_receivedMessages.Count}";
      }
      catch (Exception ex)
      {
        return $"Error during monitoring: {ex.Message}";
      }
    }

    /// <summary>
    /// Cleanup resources when the plugin is disposed
    /// </summary>
    ~SignalRPlugin()
    {
      if (_connection != null)
      {
        try
        {
          var stopTask = Task.Run(async () => await _connection.StopAsync());
          stopTask.Wait(TimeSpan.FromSeconds(2));
          var disposeTask = Task.Run(async () => await _connection.DisposeAsync());
          disposeTask.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
          // Ignore cleanup errors
        }
      }
    }
  }
}
