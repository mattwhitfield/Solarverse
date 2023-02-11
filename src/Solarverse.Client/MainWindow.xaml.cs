using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Solarverse.Core.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Solarverse.Client
{
    public partial class MainWindow : Window
    {
        private ServiceProvider _serviceProvider;
        private MainWindowViewModel _viewModel;
        private bool _closing;
        private ISolarverseApiClient _solarverseApiClient;

        public MainWindow()
        {
            InitializeComponent();

            var configuration = new ConfigurationBuilder()                
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .Build();

            IServiceCollection collection = new ServiceCollection();

            collection.Configure<ClientConfiguration>(
                configuration.GetSection("Client"));

            collection.AddLogging();

            collection.AddSingleton<IUpdateHandler>(Graph);
            collection.AddTransient<ISolarverseApiClient, SolarverseApiClient>();
            collection.AddTransient<IDataHubClient, DataHubClient>();
            collection.AddSingleton<MainWindowViewModel>();
            _serviceProvider = collection.BuildServiceProvider();

            DataContext = _viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            _solarverseApiClient = _serviceProvider.GetRequiredService<ISolarverseApiClient>();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            // TODO - observe exceptions
            Task.Run(async () => 
            {
                try
                {
                    await _viewModel.Start();
                    await _solarverseApiClient.UpdateTimeSeries();
                }
                catch (Exception e)
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("Failed while connecting to server - " + e.GetType().Name + ": " + e.Message, "Failed during startup", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                }
            });
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closing)
            {
                return;
            }
            _closing = true;

            e.Cancel = true;

            await _viewModel.Stop();
            Close();
        }
    }
}
