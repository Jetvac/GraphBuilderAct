using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static GraphBuilder.Graph;

namespace GraphBuilder
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables
        public static Graph classGraph;
        public static DBController dbController { get; set; }
        public static LP04Entities dbConnection { get; set; } = new LP04Entities();
        public static TextBox nodeName { get; set; }
        public static TextBox nodeAbbreviation { get; set; }
        public static DataGrid nodeRelatedEdgeList { get; set; }
        public static GraphicInterface GraphicController { get; set; }
        public static ResourceDictionary AppResources { get; private set; }
        private static DataGrid _projectList { get; set; }
        private static bool _isListUpdating { get; set; } = false;
        public static TextBox pathLength { get; set; }
        private static Node _selectedNode { get; set; }
        private static Grid matrix { get; set; }
        public static AdjencyMatrix matrixVisual { get; set; }
        private static bool _isNodeNameUpdating { get; set; } = false;
        private static bool _isNodeAbbreviationUpdating { get; set; } = false;
        public static ObservableCollection<GraphStructure> projectListSource = new ObservableCollection<GraphStructure>();
        #endregion

        public static void ChangeControlMode(UserInputController type)
        {
            if (GraphicController != null)
            {
                GraphicController.SwitchCurrentControlMode(type);
            }
            else
            {
                //MessageBox.Show("Не выбран проект.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            AppResources = Application.Current.Resources;

            _projectList = GraphProject;
            UpdateProjectList();

            this.DataContext = this;
            nodeName = NodeName;
            nodeRelatedEdgeList = NodeRelatedEdgeList;
            nodeAbbreviation = NodeAbbName;
            pathLength = PathLength;
            matrix = Matrix;
        }

        #region Методы работы с логикой программы
        public static void ClearTabPanel()
        {
            _selectedNode = null;
            nodeName.Text = "";
            nodeAbbreviation.Text = "";
            nodeRelatedEdgeList.ItemsSource = null;
            pathLength.Text = "";
        }
        public static void UpdateMatrix()
        {
            GraphStructure project = _projectList.SelectedItem as GraphStructure;
            if (project == null) { return; }
            matrixVisual = new AdjencyMatrix(project, matrix);
        }
        public static void UpdateProjectList()
        {
            _isListUpdating = true;
            projectListSource = ListToOBS(dbConnection.GraphStructure.ToList());
            _projectList.ItemsSource = projectListSource;
            _isListUpdating = false;
        }
        public static void UpdateSelectedNodeEdges()
        {
            nodeRelatedEdgeList.ItemsSource = null;
            nodeRelatedEdgeList.ItemsSource = dbConnection.GraphEdge.Where(c => c.BaseNodeID == _selectedNode.ID || c.AddressNodeID == _selectedNode.ID).ToList();
        }
        public static void SelectNode(Node node)
        {
            _selectedNode = node;

            nodeAbbreviation.IsEnabled = true;
            nodeAbbreviation.Text = node.Abbreviation;
            nodeName.IsEnabled = true;
            nodeName.Text = node.Name;

            _isNodeAbbreviationUpdating = false;
            _isNodeNameUpdating = false;
            UpdateSelectedNodeEdges();
        }
        public static List<Node> GetNodesFromMinPath()
        {
            List<Node> nodes = new List<Node>();
            List<Edge> path = GraphicController.ActivatedPath;
            path.Reverse();

            foreach (Edge edge in path)
            {
                if (!nodes.Contains(edge.BaseNode))
                {
                    nodes.Add(edge.BaseNode);
                }
            }

            return nodes;
        }

        /*public int[] GetCanvasSize(ref Canvas canvas, int bias)
        {
            int posX, posY, width, height;
            posX = 10000; posY = 10000; width = 0; height = 0;

            foreach (var item in canvas.Children)
            {
                double itemPosX = Canvas.GetLeft((UIElement)item);
                double itemPosY = Canvas.GetTop((UIElement)item);

                if (posX > itemPosX)
                {
                    posX = Convert.ToInt32(itemPosX);
                } 
                if (posY > itemPosY)
                {
                    posY = Convert.ToInt32(itemPosY);
                } 
                if (width < itemPosX)
                {
                    width = Convert.ToInt32(itemPosX);
                } 
                if (height < itemPosY)
                {
                    height = Convert.ToInt32(itemPosY);
                }
            }

            return new int[] { posX, posY, width, height };
        }*/
        /// <summary>
        /// Получает изображение с canvas и преобразует его в bitmap формат 
        /// </summary>
        /// <param name="canvas"></param>
        /// <returns></returns>
        public System.Drawing.Bitmap ConvertCanvas2Bitmap(ref Canvas canvas)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width,
            (int)canvas.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(canvas);
            var crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, (int)canvas.RenderSize.Width, (int)canvas.RenderSize.Height));

            System.Drawing.Bitmap resultBitmap;
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(crop));

            using (Stream s = new MemoryStream())
            {
                encoder.Save(s);
                resultBitmap = new System.Drawing.Bitmap(s);
            }

            return resultBitmap;
        }
        public System.Drawing.Bitmap GetImage()
        {
            System.Drawing.Bitmap weightBitmap = ConvertCanvas2Bitmap(ref WeightRender);
            System.Drawing.Bitmap edgeBitmap = ConvertCanvas2Bitmap(ref EdgeRender);
            System.Drawing.Bitmap nodeBitmap = ConvertCanvas2Bitmap(ref NodeRender);

            var finalImage = new System.Drawing.Bitmap((int)EdgeRender.RenderSize.Width, (int)EdgeRender.RenderSize.Height, PixelFormat.Format32bppArgb);

            var graphics = System.Drawing.Graphics.FromImage(finalImage);
            graphics.CompositingMode = CompositingMode.SourceOver;

            graphics.DrawImage(edgeBitmap, 0, 0);
            graphics.DrawImage(weightBitmap, 0, 0);
            graphics.DrawImage(nodeBitmap, 0, 0);

            return finalImage;
        }
        #endregion

        private static ObservableCollection<GraphStructure> ListToOBS(List<GraphStructure> structures)
        {
            ObservableCollection<GraphStructure> result = new ObservableCollection<GraphStructure>();
            foreach (GraphStructure structuring in structures)
                result.Add(structuring);
            return result;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isNodeNameUpdating = true;
            _isNodeAbbreviationUpdating = true;

            GraphName.Text = "Не выбран";
            GraphName.IsEnabled = false;
            NodeName.IsEnabled = false;
            NodeAbbName.IsEnabled = false;
        }

        #region События
        private void AddNodeSwitch_Click(object sender, RoutedEventArgs e)
        {
            ChangeControlMode(UserInputController.NodeCreating);
        }
        private void AddEdgeSwitch_Click(object sender, RoutedEventArgs e)
        {
            ChangeControlMode(UserInputController.EdgeCreating);
        }
        private void DefaultModeSwitch_Click(object sender, RoutedEventArgs e)
        {
            ChangeControlMode(UserInputController.Default);
        }
        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (GraphicController == null) { return; }

            switch (GraphicController.CurrentUserControlType)
            {
                case UserInputController.NodeCreating:
                    Point mousePos = e.GetPosition((Border)sender);

                    Node node = dbController.CreateNode("Без названия", mousePos.X, mousePos.Y, "NN");
                    if (node == null) { return; }
                    GraphicController.InitializeNodeGraphic(node);
                    break;
            }

            MainWindow.UpdateMatrix();
        }
        private void DeleteNodeSwitch_Click(object sender, RoutedEventArgs e)
        {
            ChangeControlMode(UserInputController.NodeDelete);
        }

        // Tab menu
        private void GraphProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearTabPanel();

            if (_isListUpdating) { _isListUpdating = false; return; }
            dbController = null;
            GraphStructure structure = GraphProject.SelectedItem as GraphStructure;
            GraphName.Text = structure.Name;
            GraphName.IsEnabled = true;

            UpdateMatrix();

            dbController = new DBController(structure.Name, structure);
            classGraph = dbController.GetGraph();

            GraphicController = new GraphicInterface(EdgeRender, NodeRender, WeightRender, ref classGraph);
        }
        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            ClearTabPanel();
            GraphName.IsEnabled = true;

            // Create an object
            dbController = new DBController("Новый проект");
            classGraph = dbController.GetGraph();
            GraphName.Text = "Новый проект";

            UpdateProjectList();
            GraphStructure created = dbConnection.GraphStructure.FirstOrDefault(c => c.GraphID == dbController._structureID);
            _projectList.SelectedItem = created;
            UpdateMatrix();
        }
        private void GraphName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Selected item
            GraphStructure project = _projectList.SelectedItem as GraphStructure;

            if (dbController == null) { return; }
            if (project == null) { return; }

            dbController.ChangeGraphName(GraphName.Text);
            projectListSource.FirstOrDefault(c => c.GraphID == project.GraphID).Name = GraphName.Text;
        }
        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (dbController == null) { return; }
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить проект?", "Уведомление", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) { return; }

            // Remove project from database
            dbController.DeleteProject();
            dbController = null;
            // Clear canvas
            GraphicController.ClearCanvas();
            GraphicController = null;

            // Clear gui objects
            ClearTabPanel();
            UpdateProjectList();
            GraphName.Text = "Не выбран";
            matrixVisual = null;
            Matrix.Children.Clear();
            GraphName.IsEnabled = false;
            NodeName.IsEnabled = false;
            NodeAbbName.IsEnabled = false;
        }
        private void NodeName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isNodeNameUpdating) { return; }
            if (_selectedNode == null) { return; }
            dbController.ChangeNodeName(_selectedNode.ID, nodeName.Text);
        }
        private void NodeAbbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isNodeAbbreviationUpdating) { return; }
            if (_selectedNode == null) { return; }
            if (nodeAbbreviation.Text.Length > 4) { nodeAbbreviation.Text = nodeAbbreviation.Text.Substring(0, 4); return; }
            dbController.ChangeNodeAbbreviation(_selectedNode.ID, nodeAbbreviation.Text);
            GraphicController.ChangeNodeAbbreviation(_selectedNode, nodeAbbreviation.Text);
        }
        private void FindMinPath_Click(object sender, RoutedEventArgs e)
        {
            ChangeControlMode(UserInputController.MinPath);
        }
        private void RemoveEdge_Click(object sender, RoutedEventArgs e)
        {
            GraphicController.TakeOffEdgeActivation();
            if (nodeRelatedEdgeList.SelectedItem == null) { return; }
            Edge edge = classGraph.FindEdgeByID((nodeRelatedEdgeList.SelectedItem as GraphEdge).EdgeID);
            dbController.DeleteEdge(edge);
            GraphicController.RemoveEdge(edge);

            UpdateSelectedNodeEdges();
        }
        private void ActivateEdge_Click_1(object sender, RoutedEventArgs e)
        {
            GraphicController.TakeOffEdgeActivation();
            if (nodeRelatedEdgeList.SelectedItem == null) { return; }
            Edge edge = classGraph.FindEdgeByID((nodeRelatedEdgeList.SelectedItem as GraphEdge).EdgeID);
            GraphicController.MakeEdgeActive(edge);
        }
        private void Word_Click(object sender, RoutedEventArgs e)
        {
            if (matrixVisual == null) { return; }

            GraphStructure project = _projectList.SelectedItem as GraphStructure;
            if (project == null) { return; }
            if (GraphicController == null) { return; }
            if (GraphicController.ActivatedPath == null) { return; }
            if (PathLength.Text.Length == 0) { return; }

            string path = "Example 1.docx";
            List<string[,]> table = matrixVisual.GetOutputMatrix();
            string nodePath = "";


            foreach (Node node in GraphicController.MinPath)
            {
                nodePath += $"{node.Abbreviation} > ";
            }

            // Удаляет лишнее " > " с конца
            nodePath = nodePath.Substring(0, nodePath.Length - 3);


            Report report = new Report();
            report.AddParagraph($"Проект: {project.Name}");
            report.AddParagraph("Матрица смежности: ");
            report.AddTable(table);
            report.AddParagraph($"Путь: {nodePath}");
            report.AddParagraph($"F = {PathLength.Text}");

            System.Drawing.Bitmap image = GetImage();
            image.Save("images.png");
            report.AddImageToBody("./images.png", image.Width, image.Height);


            report.SaveDocument(path);
        }
        private void CloseProgramm_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
        #endregion
    }
}
