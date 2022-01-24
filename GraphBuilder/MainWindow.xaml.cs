using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using static GraphBuilder.Graph;
using static GraphBuilder.GraphicWorks;

namespace GraphBuilder
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Point? _movePoint;

        //Group into separate class
        private Canvas _edgeRender { get; set; }
        private Canvas _nodeRender { get; set; }


        public static Graph graph = new Graph();

        public static ResourceDictionary AppResources { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            AppResources = Application.Current.Resources;

            _edgeRender = EdgeRender;
            _nodeRender = NodeRender;
        }

        //Drag and drop triggers (Group into separate class (GraphicController)
        public static void Node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _movePoint = e.GetPosition((Label)sender);
            ((Label)sender).CaptureMouse();
        }
        public static void Node_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _movePoint = null;
            ((Label)sender).ReleaseMouseCapture();
        }
        public static void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (_movePoint == null) return;
            var p = e.GetPosition(null) - (Vector)_movePoint.Value;
            Node selectedNode = graph.FindNodeByAdapter((Label)sender);

            //Move node on canvas
            selectedNode.MoveNode(p.X, p.Y);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            graph.graphNodes.Add(InitializeNode(_nodeRender, "D1", 10, 10));
            graph.graphNodes.Add(InitializeNode(_nodeRender, "D2", 50, 80));
            graph.graphNodes.Add(InitializeNode(_nodeRender, "D3", 70, 50));
            graph.graphNodes.Add(InitializeNode(_nodeRender, "D4", 90, 50));

            graph.graphEdges.Add(graph.graphNodes[0].AddEdge(_edgeRender, graph.graphNodes[1])); // 1 to 2
            graph.graphEdges.Add(graph.graphNodes[2].AddEdge(_edgeRender, graph.graphNodes[0])); // 3 to 1
            graph.graphEdges.Add(graph.graphNodes[2].AddEdge(_edgeRender, graph.graphNodes[1])); // 3 to 2
            graph.graphEdges.Add(graph.graphNodes[3].AddEdge(_edgeRender, graph.graphNodes[1])); // 3 to 2
        }
    }
}
