using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MemoryGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Action<object, SizeChangedEventArgs>? onSizeChanged;

        public MainWindow()
        {
            InitializeComponent();
            var dataContext = DataContext as MainWindowViewModel;
            if (dataContext is null)
            {
                return;
            }
            dataContext.Parent = this;
            this.onSizeChanged = dataContext.OnSizeChanged;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => onSizeChanged?.Invoke(sender, e);
    }
}
