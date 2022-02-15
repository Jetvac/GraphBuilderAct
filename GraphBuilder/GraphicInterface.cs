using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        public List<Edge> ActivatedPath = new List<Edge>();
        public List<Node> MinPath = new List<Node>();
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



        #region Методы инициализации объектов
        /// <summary>
        /// Инициализируте переданный граф
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
        /// Инициализирует узел, задаёт ему базовые зависимости и размещает на canvas
        /// </summary>
        /// <param name="created">Логическая структура узла</param>
        /// <returns>Созданный узел</returns>
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
        /// Инициализирует ребро, задаёт ему базовые зависимости и размещает на canvas
        /// </summary>
        /// <param name="created">Логическая структура ребра</param>
        /// <returns>Созданное ребро</returns>
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
        /// Инициализирует объект weight, задаёт ему позицию между зависимыми узлами и размещает на canvas
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
        #endregion

        #region Методы удаления объектов
        /// <summary>
        /// Удаляет узел из логической структуры графа
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Связанные с узлом рёбра</returns>
        public List<Edge> RemoveNode(Node node)
        {
            List<Edge> relatedEdges = _graphRef.RemoveNode(node);
            _nodeLayer.Children.Remove(node.VisualAdapter);

            return relatedEdges;
        }
        /// <summary>
        /// Удаляет ребро из логической структуры графа
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
        /// Удаляет ребра из логической структуры графа
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
        /// Удаляет объект weight из логической структуры графа
        /// </summary>
        /// <param name="parent"></param>
        public void RemoveEdgeRelatedWeight(Edge parent)
        {
            _weightLayer.Children.Remove(parent.WeightAdapter.VisualAdapter);
        }
        /// <summary>
        /// Удаляет все объекты, размещённые на canvas
        /// </summary>
        public void ClearCanvas()
        {
            _edgeLayer.Children.Clear();
            _nodeLayer.Children.Clear();
            _weightLayer.Children.Clear();
        }
        #endregion

        #region Методы позиционирования объектов
        /// <summary>
        /// Изменяет позицию узла на canvas
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
        /// Рассчитывает новую позицию weight по зависимостям
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
        /// Изменяет позицию первой точки ребра на canvas
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
        /// Изменяет позицию второй точки ребра на canvas
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="newPosX"></param>
        /// <param name="newPosY"></param>
        private void MoveSecondPointOfEdge(Edge edge, double newPosX, double newPosY)
        {
            edge.VisualAdapter.X2 = newPosX + WIDTH / 2;
            edge.VisualAdapter.Y2 = newPosY + HEIGHT / 2;
        }
        #endregion

        #region Методы размещения объектов на canvas
        /// <summary>
        /// Размещает узел на canvas по указаной в объекте позиции
        /// </summary>
        /// <param name="node"></param>
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
        /// Размещает ребро на canvas по указаной в объекте позиции
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
        /// Размещает вес на canvas между двумя узлами
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
        #endregion

        #region Логика
        public void ChangeNodeAbbreviation(Node node, string newName)
        {
            node.Abbreviation = newName;
            node.VisualAdapter.Content = newName;
        }
        /// <summary>
        /// Изменяет текущий режим контроля пространством
        /// </summary>
        /// <param name="type"></param>
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
        /// Изменяет стиль узла на активный
        /// </summary>
        /// <param name="node"></param>
        private void MakeNodeActive(Node node)
        {
            node.VisualAdapter.Style = (Style)MainWindow.AppResources["NodeActive"];
            _activatedNode = node;
        }
        /// <summary>
        /// Изменяет стиль ребра на активный
        /// </summary>
        /// <param name="edge"></param>
        public void MakeEdgeActive(Edge edge)
        {
            edge.VisualAdapter.Style = (Style)MainWindow.AppResources["EdgeMarked"];
            ActivatedPath.Add(edge);
        }
        /// <summary>
        /// Изменяет стиль узла на нормальный
        /// </summary>
        private void TakeOffNodeActivation()
        {
            if (_activatedNode != null)
            {
                _activatedNode.VisualAdapter.Style = (Style)MainWindow.AppResources["Node"];
                _activatedNode = null;
            }
        }
        /// <summary>
        /// Изменяет стиль ребра на нормальный
        /// </summary>
        public void TakeOffEdgeActivation()
        {
            foreach (Edge edge in ActivatedPath)
            {
                edge.VisualAdapter.Style = (Style)MainWindow.AppResources["Edge"];
            }
            ActivatedPath.Clear();
        }
        /// <summary>
        /// Получает минимальный путь между узлами с помощью алгоритма Дейкстры
        /// </summary>
        /// <param name="firstPointNode">Узел начала</param>
        /// <param name="secondPointNode">Узел конца</param>
        /// <returns></returns>
        public List<Edge> GetMinPath(Node firstPointNode, Node secondPointNode)
        {
            List<Edge> path = new List<Edge>();
            List<MarkedNodeList> nodes = new LP04Entities().MinPath(firstPointNode.ID, secondPointNode.ID).ToList();
            MinPath = new List<Node>();

            // Save end-point edge (back direction)
            int currentNodeID = secondPointNode.ID;

            MainWindow.pathLength.Text = Convert.ToString(nodes.FirstOrDefault(c => c.NodeID == currentNodeID).DistFromStart);

            while (true)
            {
                int? nextNodeID = nodes.FirstOrDefault(c => c.NodeID == currentNodeID).PrevNode;
                if (nextNodeID == null) { return null; }
                if (nextNodeID == -1) { break; } // Конечная

                Node first = _graphRef.FindNodeByID(Convert.ToInt32(nextNodeID));
                Node second = _graphRef.FindNodeByID(currentNodeID);
                MinPath.Add(second);

                path.Add(_graphRef.SearchEdgeNonStraightedByNodes(first, second));

                currentNodeID = Convert.ToInt32(nextNodeID);
            }


            MinPath.Add(firstPointNode);
            MinPath.Reverse();
            return path;
        }
        #endregion

        #region Drag and drop triggers
        /// <summary>
        /// Инициализирует объект для отслеживания позиции мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        List<Edge> path = GetMinPath(_activatedNode, selectedNode);
                        if (path == null)
                        {
                            MessageBox.Show("Путь не найден!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        foreach (Edge edge in path)
                        {
                            MakeEdgeActive(edge);
                        }

                        MainWindow.UpdateMatrix();
                        TakeOffNodeActivation();
                    }
                    break;
            }
        }
        /// <summary>
        /// Перемещает узел в случае если выставлен нужный режим
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        #endregion

        #region Другие триггеры
        /// <summary>
        /// Изменяет значение расстояние в базе и на форме
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Триггер на запрет ввода любых символов, кроме цифер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WeightTxtBx_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = "0123456789".IndexOf(e.Text) < 0;
        }
        #endregion
    }
}
