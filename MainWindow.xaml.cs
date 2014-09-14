using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SessionSpotter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : TransparentWindow
    {
        private iNotifierViewModel _notifierViewModel;
        public MainWindow()
        {
            InitializeComponent();
            _notifierViewModel = new iNotifierViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Config<UserSettings>.Load();
            LoadSettings();

            this.DataContext = _notifierViewModel;
            _notifierViewModel.Init(); 
        }

        private void LoadSettings()
        {
            var settings = Config<UserSettings>.Instance;
            if (settings.WindowRect.HasValue)
            {
                this.Left = settings.WindowRect.Value.Left;
                this.Top = settings.WindowRect.Value.Top;
                this.Width = settings.WindowRect.Value.Width;
                this.Height = settings.WindowRect.Value.Height;
            }

            this.Topmost = settings.IsWindowTopmost;
        }

        private void SaveSettings()
        {
            Config<UserSettings>.Instance.WindowRect = new Rect(this.Left, this.Top, this.Width, this.Height);
            Config<UserSettings>.Instance.IsWindowTopmost = this.Topmost;
            Config<UserSettings>.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != _notifierViewModel)
            {
                _notifierViewModel.Dispose();
            }

            SaveSettings();
        }

        private void PART_ContainerGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (!(e.OriginalSource as DependencyObject).HasValue(s => s.CheckParent(p => p is GridSplitter || p is ButtonBase)))
            {
                this.DragMove();
            }
        }
    }
}
