using GalacticCommander.ViewModels;
using GalacticCommander.Services;
using GalacticCommander.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace GalacticCommander
{
    /// <summary>
    /// Advanced WPF Application with Dependency Injection, Logging, and Service Architecture
    /// Demonstrates: DI Container, Service Locator Pattern, Application Lifecycle Management
    /// </summary>
    public partial class App : Application
    {
        internal IHost? _host;
        
        /// <summary>
        /// Application startup - configure services and dependency injection
        /// Shows advanced application architecture with proper service registration
        /// </summary>
        protected override async void OnStartup(StartupEventArgs e)
        {
            // Configure the host with dependency injection and logging
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Core Services
                    services.AddSingleton<IGameEngine, GameEngine>();
                    services.AddSingleton<IInputService, InputService>();
                    services.AddSingleton<IAudioService, AudioService>();
                    services.AddSingleton<IAssetService, AssetService>();
                    services.AddSingleton<IPhysicsService, PhysicsService>();
                    services.AddSingleton<IEntityManager, EntityManager>();
                    
                    // Game Services
                    services.AddSingleton<IScoreService, ScoreService>();
                    services.AddSingleton<IGameStateService, GameStateService>();
                    services.AddSingleton<IParticleSystem, ParticleSystem>();
                    services.AddSingleton<ISerializationService, SerializationService>();
                    
                    // Entity Factory
                    services.AddSingleton<IEntityFactory, EntityFactory>();
                    
                    // ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<GameViewModel>();
                    services.AddTransient<MenuViewModel>();
                    
                    // Views
                    services.AddTransient<MainWindow>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            await _host.StartAsync();

            // Get the main window from DI container
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        /// <summary>
        /// Proper cleanup when application exits
        /// Demonstrates resource management and async disposal
        /// </summary>
        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            
            base.OnExit(e);
        }

        /// <summary>
        /// Global exception handling for unhandled exceptions
        /// Shows advanced error handling and logging
        /// </summary>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = _host?.Services.GetService<ILogger<App>>();
            logger?.LogError(e.Exception, "Unhandled exception occurred");
            
            MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}", 
                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true;
        }
    }
}