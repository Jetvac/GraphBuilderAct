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
            visualAdapter.TextChanged += WeightTxtBx_TextChanged;
            Weight result = new Weight(weight, visualAdapter);

            return result;
        }
        

        //Object delete methods
        /// <summary>
        /// Delete node from references graph logic structure
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Related edges with removed node</returns>
        public List<Edge> RemoveNode(Node node)
        {
            List<Edge> relatedEdges = _graphRef.RemoveNode(node);
            _nodeLayer.Children.Remove(node.VisualAdapter);

            return relatedEdges;
        }
        public void RemoveNodeRelatedEdges(List<Edge> edges)
        {
            foreach (Edge item in edges)
            {
                _edgeLayer.Children.Remove(item.VisualAdapter);
                RemoveEdgeRelatedWeight(item);
            }
        }
        public void RemoveEdgeRelatedWeight(Edge parent)
        {
            _weightLayer.Children.Remove(parent.WeightAdapter.VisualAdapter);
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

            List<Edge> doublewayEdges = node.doublewayEdges;

            //Move edge on canvas (First Position)
            foreach (Edge edge in node.baseEdges)
            {
                if (!doublewayEdges.Contains(edge))
                {
                    MoveFirstPointOfEdge(edge, newPosX, newPosY);
                    MoveWeightAdapter(edge);
                }
            }

            //Move edge on canvas (Second Position)
            foreach (Edge edge in node.addressEdges)
            {
                if (!doublewayEdges.Contains(edge))
                {
                    MoveSecondPointOfEdge(edge, newPosX, newPosY);
                    MoveWeightAdapter(edge);
                }
            }

            //Move edge on canvas (Doubleway edges)
            foreach (Edge edge in doublewayEdges)
            {
                MoveFirstPointOfEdge(edge, newPosX, newPosY);
                Node addressNode = _graphRef.SearchNodeByEdgeAndNode(edge, node);
                MoveSecondPointOfEdge(edge, addressNode.PosX, addressNode.PosY);
                MoveWeightAdapter(edge, node, addressNode);
            }
        }
        //TODO: Add central positioning
        private void MoveWeightAdapter(Edge edge, Node baseNode = null, Node addressNode = null)
        {
            if (baseNode == null) { baseNode = _graphRef.FindBaseNodeByEdge(edge); }
            if (addressNode == null) { addressNode = _graphRef.FindAdressNodeByEdge(edge); }

            
            Canvas.SetLeft(edge.WeightAdapter.VisualAdapter, 
                ((baseNode.PosX + addressNode.PosX) / 2) + (WIDTH / 2) - (edge.WeightAdapter.VisualAdapter.ActualWidth / 2));
            Canvas.SetTop(edge.WeightAdapter.VisualAdapter, 
                ((baseNode.PosY + addressNode.PosY) / 2) + (WIDTH / 2) - (edge.WeightAdapter.VisualAdapter.ActualHeight / 2));
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
                        Edge correctWayEdge = _graphRef.SearchEdgeByNodes(_activatedNode, selectedNode);
                        Edge reversedEdge = _graphRef.SearchEdgeByNodes(selectedNode, _activatedNode);

                        //if node are equal take off activated and stop execution
                        if (_activatedNode == selectedNode) { TakeOffNodeActivation(); return; }
                        //If edge exists
                        if (correctWayEdge != null)
                        {
                            MessageBox.Show("Предлагаемое ребро существует");
                        } else if (reversedEdge != null && correctWayEdge == null) //If edge have only one way
                        {
                            _activatedNode.addressEdges.Remove(reversedEdge);
                            selectedNode.baseEdges.Remove(reversedEdge);

                            _activatedNode.doublewayEdges.Add(reversedEdge);
                            selectedNode.doublewayEdges.Add(reversedEdge);

                            reversedEdge.way = EdgeTypes.Doubleway;
                        } else if (reversedEdge != null && correctWayEdge != null) //If edge already doubleway
                        {
                            MessageBox.Show("Предлагаемое ребро существует");
                        } else //If all edges equal null
                        {
                            //Create new edge
                            AddEdge(_activatedNode, selectedNode, 100);
                        }
                        TakeOffNodeActivation();
                    }
                    break;
                case UserInputController.NodeDelete:
                    List<Edge> trashEdges = RemoveNode(selectedNode);
                    RemoveNodeRelatedEdges(trashEdges);
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


        //Another triggers
        private void WeightTxtBx_TextChanged(object sender, TextChangedEventArgs e)
        {

            //If doubleway function get additional info
            MoveWeightAdapter(_graphRef.FindEdgeByWeightAdapter((TextBox)sender));
        }
    }
}
