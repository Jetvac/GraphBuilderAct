using System.Windows;
using System.Windows.Controls;

namespace GraphBuilder
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Graph Graph = new Graph();
        public static GraphicInterface GraphicController { get; set; }
        public static ResourceDictionary AppResources { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            AppResources = Application.Current.Resources;

            GraphicController = new GraphicInterface(EdgeRender, NodeRender, WeightRender,  ref Graph);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        //Events
        private void AddNodeSwitch_Click(object sender, RoutedEventArgs e)
        {
            GraphicController.SwitchCurrentControlMode(UserInputController.NodeCreating);
        }
        private void AddEdgeSwitch_Click(object sender, RoutedEventArgs e)
        {
            GraphicController.SwitchCurrentControlMode(UserInputController.EdgeCreating);
        }
        private void DefaultModeSwitch_Click(object sender, RoutedEventArgs e)
        {
            GraphicController.SwitchCurrentControlMode(UserInputController.Default);
        }
        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (GraphicController.CurrentUserControlType)
            {
                case UserInputController.Default:
                    break;
                case UserInputController.NodeCreating:
                    Point mousePos = e.GetPosition((Border)sender);

                    GraphicController.InitializeNode("F1",  mousePos.X, mousePos.Y);
                    break;
            }
        }
        private void DeleteNodeSwitch_Click(object sender, RoutedEventArgs e)
        {
            GraphicController.SwitchCurrentControlMode(UserInputController.NodeDelete);
        }
    }
}
