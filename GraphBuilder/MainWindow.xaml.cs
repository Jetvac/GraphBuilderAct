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
        private Canvas _workplace { get; set; }

        public static ResourceDictionary AppResources { get; private set; }

        public static List<Node> graphNodes { get; set; } = new List<Node>();
        public static List<Edge> graphEdges { get; set; } = new List<Edge>();

        public MainWindow()
        {
            InitializeComponent();
            AppResources = Application.Current.Resources;

            _workplace = Workplace;
        }

        /// <summary>
        /// Search node in graph list with same visualAdapter signature
        /// </summary>
        /// <param name="visualAdapter"></param>
        /// <returns>Node item</returns>
        public static Node FindNodeByAdapter(Label visualAdapter)
        {
            return graphNodes.Where(c => c.VisualAdapter.Equals(visualAdapter)).First();
        }

        /// <summary>
        /// Search edge in graph list with same visualAdapter signature
        /// </summary>
        /// <param name="visualAdapter"></param>
        /// <returns>Edge item</returns>
        public static Edge FindEdgeByAdapter(Line visualAdapter)
        {
            return graphEdges.Where(c => c.VisualAdapter.Equals(visualAdapter)).First();
        }

        //Drag and drop triggers
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
            Node selectedNode = FindNodeByAdapter((Label)sender);

            //Move node on canvas
            selectedNode.ChangePosition(p.X, p.Y);
            Canvas.SetLeft(selectedNode.VisualAdapter, p.X);
            Canvas.SetTop(selectedNode.VisualAdapter, p.Y);

            //Move edge on canvas (Start Position)
            foreach (Edge edge in selectedNode.baseEdges)
            {
                edge.ChangeStartPosition(p.X, p.Y);
            }

            //Move edge on canvas (End Position)
            foreach (Edge edge in selectedNode.addressEdges)
            {
                edge.ChangeEndPosition(p.X, p.Y);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            graphNodes.Add(new Node(10, 10, "Д1"));
            graphNodes.Add(new Node(100, 100, "Д2"));

            graphEdges.Add(new Edge(graphNodes[0], graphNodes[1])); // 1 to 2
            graphEdges.Add(new Edge(graphNodes[1], graphNodes[0])); // 2 to 1

            PlaceEdge(_workplace, graphNodes[0].baseEdges[0]);
            PlaceEdge(_workplace, graphNodes[1].baseEdges[0]);

            PlaceNode(_workplace, graphNodes[0]);
            PlaceNode(_workplace, graphNodes[1]);
        }
    }
}
