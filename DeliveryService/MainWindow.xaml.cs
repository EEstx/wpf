using System.Windows;
using DeliveryService.ViewModels;

namespace DeliveryService
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}