using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using static GraphBuilder.Graph;

namespace GraphBuilder
{
    public class GraphicInterface
    {
        //Base element size
        const int WIDTH = 40;
        const int HEIGHT = 40;

        private Canvas _edgeLayer { get; set; }
        private Canvas _nodeLayer { get; set; }
        //Initialize an object that using for dedicating of mouse position
        private Point? _movePoint;
        private Graph _graphRef { get; set; }
        private Node _activatedNode { get; set; }

        //Used for user control logic
        public UserInputController CurrentUserControlType { get; set; } = UserInputController.Default;



        public GraphicInterface(Canvas edgeLayer, Canvas nodeLayer, ref Graph graph)
        {
            _edgeLayer = edgeLayer;
            _nodeLayer = nodeLayer;

            _graphRef = graph;
        }

        //Graphic
        /// <summary>
        /// Initialize node, set basic dependencies and place it on canvas
        /// </summary>
        /// <returns>Created node</returns>
        public Node InitializeNode(string name, double posX, double posY)
        {
            Node created = new Node(name, posX, posY);

            //Create label object and add basic event dependencies
            created.VisualAdapter = new Label()
            { Style = (Style)MainWindow.AppResources["Node"], Width = WIDTH, Height = HEIGHT, Content = name };
            created.VisualAdapter.MouseUp += Node_MouseUp;
            created.VisualAdapter.MouseDown += Node_MouseDown;
            created.VisualAdapter.MouseMove += Node_MouseMove;

            //Place label on canvas
            PlaceNode(created);

            return created;
        }

        /// <summary>
        /// Initialize and add edge to node, set basic dependencies and place it on canvas
        /// </summary>
        /// <param name="edgeGraphicLayer"></param>
        /// <param name="baseNode"></param>
        /// <param name="addressNode"></param>
        /// <returns></returns>
        public Edge AddEdge(Node baseNode, Node addressNode)
        {
            Edge created = new Edge(baseNode, addressNode);
            created.VisualAdapter = new Line() { Style = (Style)MainWindow.AppResources["Edge"] };

            //Place edge on canvas
            PlaceEdge(created, baseNode, addressNode);

            return created;
        }


        /// <summary>
        /// Change node position on canvas
        /// </summary>
        /// <param name="node"></param>
        /// <param name="newPosX"></param>
        /// <param name="newPosY"></param>
        public void MoveNode(Node node, double newPosX, double newPosY)
        {
            node.PosX = newPosX;
            node.PosY = newPosY;

            //Move node on canvas
            Canvas.SetLeft(node.VisualAdapter, newPosX);
            Canvas.SetTop(node.VisualAdapter, newPosY);

            //Move edge on canvas (First Position)
            foreach (Edge edge in node.baseEdges)
            {
                MoveFirstPointOfEdge(edge, newPosX, newPosY);
            }

            //Move edge on canvas (Second Position)
            foreach (Edge edge in node.addressEdges)
            {
                MoveSecondPointOfEdge(edge, newPosX, newPosY);
            }
        }

        /// <summary>
        /// Change position of first point of edge on canvas
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="newPosX"></param>
        /// <param name="newPosY"></param>
        private void MoveFirstPointOfEdge(Edge edge, double newPosX, double newPosY)
        {
            edge.VisualAdapter.X1 = newPosX + WIDTH / 2;
            edge.VisualAdapter.Y1 = newPosY + HEIGHT / 2;
        }

        /// <summary>
        /// Change position of second point of edge on canvas
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="newPosX"></param>
        /// <param name="newPosY"></param>
        private void MoveSecondPointOfEdge(Edge edge, double newPosX, double newPosY)
        {
            edge.VisualAdapter.X2 = newPosX + WIDTH / 2;
            edge.VisualAdapter.Y2 = newPosY + HEIGHT / 2;
        }

        /// <summary>
        /// Place node on canvas by given position
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="node"></param>
        /// <param name="pointX"></param>
        /// <param name="pointY"></param>
        /// <returns></returns>
        private bool PlaceNode(Node node)
        {
            //check data
            if (node == null) return false;
            if (_nodeLayer.Children.Contains(node.VisualAdapter)) { _nodeLayer.Children.Remove(node.VisualAdapter); }

            _nodeLayer.Children.Add(node.VisualAdapter);
            Canvas.SetLeft(node.VisualAdapter, node.PosX);
            Canvas.SetTop(node.VisualAdapter, node.PosY);

            return true;
        }

        /// <summary>
        /// Place edge on canvas
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="baseNode"></param>
        /// <param name="addressNode"></param>
        /// <returns></returns>
        private bool PlaceEdge(Edge edge, Node baseNode, Node addressNode)
        {
            //check data
            if (edge == null || baseNode == null || addressNode == null) return false;

            MoveFirstPointOfEdge(edge, baseNode.PosX, baseNode.PosY);
            MoveSecondPointOfEdge(edge, addressNode.PosX, addressNode.PosY);
            _edgeLayer.Children.Add(edge.VisualAdapter);

            return true;
        }


        //Logic
        public void SwitchCurrentControlMode(UserInputController type)
        {
            switch (type)
            {
                case UserInputController.Default:
                    TakeOffNodeActivation();
                    CurrentUserControlType = UserInputController.Default;
                    break;
                case UserInputController.NodeCreating:
                    TakeOffNodeActivation();
                    CurrentUserControlType = UserInputController.NodeCreating;
                    break;
                case UserInputController.EdgeCreating:
                    CurrentUserControlType = UserInputController.EdgeCreating;
                    break;
            }
        }
        /// <summary>
        /// Change node adapter style to active
        /// </summary>
        /// <param name="node"></param>
        private void MakeNodeActive(Node node)
        {
            node.VisualAdapter.Style = (Style)MainWindow.AppResources["NodeActive"];
            _activatedNode = node;
        }
        /// <summary>
        /// Change node adapter style to normal
        /// </summary>
        private void TakeOffNodeActivation()
        {
            if (_activatedNode != null)
            {
                _activatedNode.VisualAdapter.Style = (Style)MainWindow.AppResources["Node"];
                _activatedNode = null;
            }
        }


        //Drag and drop triggers
        public void Node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _movePoint = e.GetPosition((Label)sender);
            ((Label)sender).CaptureMouse();
        }
        public void Node_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Node selectedNode = _graphRef.FindNodeByAdapter((Label)sender);

            _movePoint = null;
            ((Label)sender).ReleaseMouseCapture();

            switch (CurrentUserControlType)
            {
                case UserInputController.EdgeCreating:
                    if (_activatedNode == null)
                    {
                        MakeNodeActive(selectedNode);
                    }
                    else
                    {
                        // Were selected another node
                        if (_activatedNode != selectedNode)
                        {
                            _graphRef.graphEdges.Add(AddEdge(_activatedNode, selectedNode));
                        }
                        TakeOffNodeActivation();
                    }
                    break;
            }
        }
        public void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (_movePoint == null) return;
            var p = e.GetPosition(null) - (Vector)_movePoint.Value;
            Node selectedNode = _graphRef.FindNodeByAdapter((Label)sender);


            switch (CurrentUserControlType)
            {
                case UserInputController.Default:
                    MoveNode(selectedNode, p.X, p.Y);
                    break;

                case UserInputController.NodeCreating:
                    break;
            }
        }
    }
}
