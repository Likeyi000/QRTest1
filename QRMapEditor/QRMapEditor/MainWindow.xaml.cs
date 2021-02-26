using System.Windows;
using System.Windows.Controls;
using QRMapEditor;

namespace QRMapEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Title = "QRMapEditor_V0.9.2";
            this.Name = "EditorMain";
            this.WindowState = WindowState.Maximized;   //窗体最大化
        }
    }
}
