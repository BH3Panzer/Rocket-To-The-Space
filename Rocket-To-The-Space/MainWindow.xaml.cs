using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Rocket_To_The_Space
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            Cursor = Cursors.None;
            ShowMainMenu();
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += UpdateCursor;
            timer.Start();
        }

        private void UpdateCursor(object? sender, EventArgs e)
        {
            Point mousePos = Mouse.GetPosition(mainWindow);
        }

        private void ShowMainMenu()
        {
            UCMainMenu mainMenu = new UCMainMenu();
            this.Content = mainMenu;
            mainMenu.PlayButton.Click += ShowRuleScreen;
        }

        private void ShowRuleScreen(object sender, RoutedEventArgs e)
        {
            UCGameRules gameRules = new UCGameRules();
            this.Content = gameRules;
        }
    }
}