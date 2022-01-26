using System.Collections.Generic;
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
        private Canvas _weightLayer { get; set; }

        //Initialize an object that using for dedicating of mouse position
        private Point? _movePoint;
        private Graph _graphRef { get; set; }
        private Node _activatedNode { get; set; }

        //Used for user control logic
        public UserInputController CurrentUserControlType { get; set; } = UserInputController.Default;
        public GraphicInterface(Canvas edgeLayer, Canvas nodeLayer, Canvas weightLayer, ref Graph graph)
        {
            _edgeLayer = edgeLayer;
            _nodeLayer = nodeLayer;
            _weightLayer = weightLayer;

            _graphRef = graph;
        }




        //Object initialize methods
        /// <summary>
        /// Initialize node, set basic dependencies and place it on canvas
        /// </summary>
        /// <returns>Created node</returns>
        public Node InitializeNode(string name, double posX, double posY)
        {
            Node created = _graphRef.CreateBaseNode(name, posX, posY);

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
        public Edge AddEdge(Node baseNode, Node addressNode, double value)
        {
            Weight weightAdapter = CreateWeightAdapter(value);

            Edge created = _graphRef.CreateBaseEdge(baseNode, addressNode, weightAdapter);
            created.VisualAdapter = new Line() { Style = (Style)MainWindow.AppResources["Edge"] };

            //Place edge on canvas 
            PlaceEdge(created, baseNode, addressNode);
            //Place weight on canvas (Strict after placing edge)
            PlaceWeight(weightAdapter, created);

            return created;
        }
        /// <summary>
        /// Intitialize and add weight to edge, set basic dependencies and place it on canvas
        /// </summary>
        /// <param name="weight"></param>
        /// <returns></returns>
        public Weight CreateWeightAdapter(double weight)
        {
            TextBox visualAdapter = new TextBox() { Text = weight.ToString() };
            Weight result = new Weight(weight, visualAdapter);

            return result;
        }

        //Object delete methods
        public void RemoveNode(Node node)
        {
            _nodeLayer.Children.Remove(node.VisualAdapter);
        }
        public void RemoveNodeRelatedEdges(Node clearingNode)
        {
            List<Edge> removed = _graphRef.RemoveNode(clearingNode);
            foreach (Edge item in removed)
                _edgeLayer.Children.Remove(item.VisualAdapter);
        }

        //Object positioning methods
        /// <summary>
        /// Change node position on canvas
        /// </summary>
        /// <param name="node"></param>
        /// <param name="newPosX"></param>
        /// <param name="newPosY"></param>
        public void MoveNodeStructure(Node node, double newPosX, double newPosY)
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
                MoveWeightAdapter(edge);
            }

            //Move edge on canvas (Second Position)
            foreach (Edge edge in node.addressEdges)
            {
                MoveSecondPointOfEdge(edge, newPosX, newPosY);
                MoveWeightAdapter(edge);
            }
        }
        //TODO: Add central positioning
        private void MoveWeightAdapter(Edge parentEdge)
        {
            Canvas.SetLeft(parentEdge.WeightAdapter.VisualAdapter, 
                ((parentEdge.BaseNode.PosX + parentEdge.AddressNode.PosX) / 2) + WIDTH / 2);
            Canvas.SetTop(parentEdge.WeightAdapter.VisualAdapter, 
                ((parentEdge.BaseNode.PosY + parentEdge.AddressNode.PosY) / 2) + WIDTH / 2);
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


        //Object place methods (On canvas)
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
        /// <summary>
        /// Place weight on canvas at center of two nodes (Base and parent)
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="parentEdge"></param>
        /// <returns></returns>
        private bool PlaceWeight(Weight weight, Edge parentEdge)
        {
            _weightLayer.Children.Add(weight.VisualAdapter);
            MoveWeightAdapter(parentEdge);

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
                case UserInputController.NodeDelete:
                    CurrentUserControlType = UserInputController.NodeDelete;
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
                        // Were selected another node and check existance
                        if (_activatedNode != selectedNode && _graphRef.SearchEdgeByNodes(_activatedNode, selectedNode) == null)
                        {
                            AddEdge(_activatedNode, selectedNode, 100);
                        } else
                        {
                            MessageBox.Show("Предлагаемое ребро существует", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        TakeOffNodeActivation();
                    }
                    break;
                case UserInputController.NodeDelete:
                    
                    
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
                    MoveNodeStructure(selectedNode, p.X, p.Y);
                    break;

                case UserInputController.NodeCreating:
                    break;
            }
        }
    }
}
