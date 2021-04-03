using System.Windows.Controls;

namespace CalculaWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly MainWindowViewModel vm;

        public MainWindow()
        {
            vm = new MainWindowViewModel();
            vm.LoadHistory();
            DataContext = vm;

            InitializeComponent();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            using (tb.DeclareChangeBlock())
            {
                foreach (var c in e.Changes)
                {
                    if (c.AddedLength == 0) continue;
                    tb.Select(c.Offset + c.AddedLength, 0);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm.SaveHistory();
        }
    }
}
