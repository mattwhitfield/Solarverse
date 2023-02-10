using Microsoft.Extensions.DependencyInjection;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using System.Windows;

namespace Solarverse
{
    public partial class MainWindow : Window
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly MainWindowViewModel _viewModel;

        bool _closing = false;

        public MainWindow()
        {
            InitializeComponent();

            var configuration = StaticConfigurationProvider.Configuration;

            Graph.ShowDevelopmentDetail = configuration.TestMode;
            Graph.BatteryCapacityKwh = configuration.Battery.CapacityKwh ?? 5.2;

            var collection = ServiceCollectionFactory.Create();
            collection.AddSingleton<ITimeSeriesHandler>(Graph);
            collection.AddSingleton<MainWindowViewModel>();
            _serviceProvider = collection.BuildServiceProvider();

            DataContext = _viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            _viewModel.Start();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closing)
            {
                return;
            }
            _closing = true;

            ShuttingDownGrid.Visibility = Visibility.Visible;

            e.Cancel = true;

            await _viewModel.Stop();
            Close();
        }
    }
}
