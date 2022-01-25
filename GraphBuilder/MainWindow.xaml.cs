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

            GraphicController = new GraphicInterface(EdgeRender, NodeRender, ref Graph);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Graph.graphNodes.Add(GraphicController.InitializeNode("D1", 10, 10));
            Graph.graphNodes.Add(GraphicController.InitializeNode("D2", 50, 80));
            Graph.graphNodes.Add(GraphicController.InitializeNode("D3", 70, 50));
            Graph.graphNodes.Add(GraphicController.InitializeNode("D4", 90, 50));

            Graph.graphEdges.Add(GraphicController.AddEdge(Graph.graphNodes[0], Graph.graphNodes[1])); // 1 to 2
            Graph.graphEdges.Add(GraphicController.AddEdge(Graph.graphNodes[2], Graph.graphNodes[0])); // 3 to 1
            Graph.graphEdges.Add(GraphicController.AddEdge(Graph.graphNodes[2], Graph.graphNodes[1])); // 3 to 2
            Graph.graphEdges.Add(GraphicController.AddEdge(Graph.graphNodes[3], Graph.graphNodes[1])); // 3 to 2
        }

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

                    Graph.graphNodes.Add(GraphicController.InitializeNode("F1",  mousePos.X, mousePos.Y));
                    break;
            }
        }
    }
}
