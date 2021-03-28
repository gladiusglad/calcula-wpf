using System.Windows.Controls;
using System.Windows.Media;

namespace WPFCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly ViewModel vm;

        public MainWindow()
        {
            vm = new ViewModel();
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

        private void FadeNormalCompleted(object sender, System.EventArgs e)
        {
            ChangeResultColor("Normal");
        }

        private void FadeWarningCompleted(object sender, System.EventArgs e)
        {
            ChangeResultColor("Warning");
        }

        private void FadeUnfocusedCompleted(object sender, System.EventArgs e)
        {
            ChangeResultColor("Unfocused");
        }

        private void ChangeResultColor(string colorResource)
        {
            if (!((SolidColorBrush)ResultText.Foreground).IsFrozen)
            {
                ((SolidColorBrush)ResultText.Foreground).Color = (Color)TryFindResource(colorResource);
            }
        }

    }
}
