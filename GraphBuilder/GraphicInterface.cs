using System;
using System.Collections.Generic;
using System.Linq;
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
        private Point? _movePoint;
        private Graph _graphRef { get; set; }
        private Node _activatedNode { get; set; }
        private List<Edge> _activatedPath = new List<Edge>();

        //Used for user control logic
        public UserInputController CurrentUserControlType { get; set; } = UserInputController.Default;
        public GraphicInterface(Canvas edgeLayer, Canvas nodeLayer, Canvas weightLayer, ref Graph graph)
        {
            _edgeLayer = edgeLayer;
            _nodeLayer = nodeLayer;
            _weightLayer = weightLayer;

            ClearCanvas();

            _graphRef = graph;
            InitializeGraphStructure(_graphRef);
        }



        //Object initialize methods
        /// <summary>
        /// Place graph objects on canvas
        /// </summary>
        /// <param name="graph"></param>
        public void InitializeGraphStructure(Graph graph)
        {
            foreach (Node node in graph.graphNodes)
            {
                InitializeNodeGraphic(node);
            }

            foreach (Edge edge in graph.graphEdges)
            {
                InitializeEdgeGraphic(edge);
            }
        }
        /// <summary>
        /// Initialize node, set basic dependencies and place it on canvas
        /// </summary>
        /// <returns>Created node</returns>
        public Node InitializeNodeGraphic(Node created)
        {
            //Create label object and add basic event dependencies
            created.VisualAdapter = new Label()
            { Style = (Style)MainWindow.AppResources["Node"], Width = WIDTH, Height = HEIGHT, Content = created.Abbreviation };
            created.VisualAdapter.MouseUp += Node_MouseUp;
            created.VisualAdapter.MouseDown += Node_MouseDown;
            created.VisualAdapter.MouseMove += Node_MouseMove;

            //Place label on canvas
            PlaceNode(created);

            return created;
        }
        /// <summary>
        /// Initialize edge, set basic dependencies and place it on canvas
        /// </summary>
        /// <param name="edgeGraphicLayer"></param>
        /// <param name="baseNode"></param>
        /// <param name="addressNode"></param>
        /// <returns></returns>
        public Edge InitializeEdgeGraphic(Edge created)
        {
            Weight weightAdapter = CreateWeightAdapter(created.Weight);

            created.WeightAdapter = weightAdapter;
            created.VisualAdapter = new Line() { Style = (Style)MainWindow.AppResources["Edge"] };

            //Place edge on canvas 
            PlaceEdge(created, created.BaseNode, created.AddressNode);
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
            visualAdapter.PreviewTextInput += WeightTxtBx_PreviewTextInput;
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
        /// <summary>
        /// Delete edge from references graph logic structure
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public void RemoveEdge(Edge edge)
        {
            _graphRef.RemoveEdge(edge);
            _edgeLayer.Children.Remove(edge.VisualAdapter);
            RemoveEdgeRelatedWeight(edge);
        }
        /// <summary>
        /// Delete edges by depended node
        /// </summary>
        /// <param name="edges"></param>
        public void RemoveNodeRelatedEdges(List<Edge> edges)
        {
            foreach (Edge item in edges)
            {
                _edgeLayer.Children.Remove(item.VisualAdapter);
                RemoveEdgeRelatedWeight(item);
            }
        }
        /// <summary>
        /// Delete weight by depended weight
        /// </summary>
        /// <param name="parent"></param>
        public void RemoveEdgeRelatedWeight(Edge parent)
        {
            _weightLayer.Children.Remove(parent.WeightAdapter.VisualAdapter);
        }
        /// <summary>
        /// Delete all children in canvas
        /// </summary>
        public void ClearCanvas()
        {
            _edgeLayer.Children.Clear();
            _nodeLayer.Children.Clear();
            _weightLayer.Children.Clear();
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
        /// <summary>
        /// Calculate new weight position by dependencies
        /// </summary>
        /// <param name="parentEdge"></param>
        private void MoveWeightAdapter(Edge parentEdge)
        {
            Canvas.SetLeft(parentEdge.WeightAdapter.VisualAdapter,
                ((parentEdge.BaseNode.PosX + parentEdge.AddressNode.PosX) / 2) + (WIDTH / 2) - (parentEdge.WeightAdapter.VisualAdapter.ActualWidth / 2));
            Canvas.SetTop(parentEdge.WeightAdapter.VisualAdapter,
                ((parentEdge.BaseNode.PosY + parentEdge.AddressNode.PosY) / 2) + (WIDTH / 2) - (parentEdge.WeightAdapter.VisualAdapter.ActualHeight / 2));
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
        public void ChangeNodeAbbreviation(Node node, string newName)
        {
            node.Abbreviation = newName;
            node.VisualAdapter.Content = newName;
        }
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
                    TakeOffNodeActivation();
                    CurrentUserControlType = UserInputController.EdgeCreating;
                    break;
                case UserInputController.NodeDelete:
                    TakeOffNodeActivation();
                    CurrentUserControlType = UserInputController.NodeDelete;
                    break;
                case UserInputController.MinPath:
                    TakeOffNodeActivation();
                    CurrentUserControlType = UserInputController.MinPath;
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
        /// Change edge adapter style to marked
        /// </summary>
        /// <param name="edge"></param>
        public void MakeEdgeActive(Edge edge)
        {
            edge.VisualAdapter.Style = (Style)MainWindow.AppResources["EdgeMarked"];
            _activatedPath.Add(edge);
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
        public void TakeOffEdgeActivation()
        {
            foreach (Edge edge in _activatedPath)
            {
                edge.VisualAdapter.Style = (Style)MainWindow.AppResources["Edge"];
            }
            _activatedPath.Clear();
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

            MainWindow.SelectNode(selectedNode);

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
                        if (_activatedNode != selectedNode &&
                            _graphRef.SearchEdgeByNodes(_activatedNode, selectedNode) == null && // Straight way
                            _graphRef.SearchEdgeByNodes(selectedNode, _activatedNode) == null)   // Reversed
                        {
                            // There is used static variable from main class
                            Edge edge = MainWindow.dbController.CreateEdge(_activatedNode, selectedNode, 0);
                            if (edge == null) { return; }
                            InitializeEdgeGraphic(edge);
                        } else
                        {
                            MessageBox.Show("Предлагаемое ребро существует", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        MainWindow.UpdateMatrix();
                        TakeOffNodeActivation();
                    }
                    break;
                case UserInputController.NodeDelete:
                    // There is used static variable from main class
                    MainWindow.dbController.DeleteNode(selectedNode);
                    
                    List<Edge> trashEdges = RemoveNode(selectedNode);
                    RemoveNodeRelatedEdges(trashEdges);
                    MainWindow.UpdateMatrix();
                    break;
                case UserInputController.MinPath:
                    TakeOffEdgeActivation();
                    if (_activatedNode == null)
                    {
                        MakeNodeActive(selectedNode);
                    }
                    else
                    {
                        List<MarkedNodeList> nodes = new LP04Entities().MinPath(_activatedNode.ID, selectedNode.ID).ToList();

                        // Save end-point edge (back direction)
                        int currentNodeID = selectedNode.ID;

                        MainWindow.pathLength.Text = Convert.ToString(nodes.FirstOrDefault(c => c.NodeID == currentNodeID).DistFromStart);

                        while (true)
                        {
                            int? nextNodeID = nodes.FirstOrDefault(c => c.NodeID == currentNodeID).PrevNode;
                            if (nextNodeID == null) { MessageBox.Show("Путь не найден!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information); break; }
                            if (nextNodeID == -1) { break; } // Конечная

                            Node first = _graphRef.FindNodeByID(Convert.ToInt32(nextNodeID));
                            Node second = _graphRef.FindNodeByID(currentNodeID);

                            MakeEdgeActive(_graphRef.SearchEdgeNonStraightedByNodes(first, second));
                            currentNodeID = Convert.ToInt32(nextNodeID);
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
                    MoveNodeStructure(selectedNode, p.X, p.Y - 40);

                    // There is used static variable from main class
                    MainWindow.dbController.MoveNode(selectedNode, p.X, p.Y - 40);
                    break;
            }
        }


        //Another triggers
        private void WeightTxtBx_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox current = (TextBox)sender;
            if (current.Text.Length == 0) { current.Text = "0"; }
            if (current.Text.Length > 4) { 
                MessageBox.Show("Максимальная длина пути не должна превышать 4-х значного числа.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error); 
                current.Text = "0"; }

            int weight = 0;

            try
            {
                weight = Convert.ToInt32(((TextBox)sender).Text);
            } catch (Exception ex)
            {
                current.Text = "0";
                return;
            }

            Edge edge = _graphRef.FindEdgeByWeightAdapter((TextBox)sender);
            // There is used static variable from main class
            MainWindow.dbController.ChangeEdgeWeightValue(edge, weight);

            MoveWeightAdapter(edge);
        }
        private void WeightTxtBx_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = "0123456789".IndexOf(e.Text) < 0;
        }
    }
}
