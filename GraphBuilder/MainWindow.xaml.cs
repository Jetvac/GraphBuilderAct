using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private static AdjencyMatrix matrixVisual { get; set; }
        private static bool _isNodeNameUpdating { get; set; } = false;
        private static bool _isNodeAbbreviationUpdating { get; set; } = false;
        public static ObservableCollection<GraphStructure> projectListSource = new ObservableCollection<GraphStructure>();

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
        public System.Drawing.Bitmap GetImage()
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)WeightRender.RenderSize.Width,
            (int)WeightRender.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(WeightRender);
            var crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, (int)WeightRender.RenderSize.Width, (int)WeightRender.RenderSize.Height));

            RenderTargetBitmap nb = new RenderTargetBitmap((int)EdgeRender.RenderSize.Width,
            (int)EdgeRender.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            nb.Render(EdgeRender);
            var cropEdge = new CroppedBitmap(nb, new Int32Rect(0, 0, (int)EdgeRender.RenderSize.Width, (int)EdgeRender.RenderSize.Height));

            RenderTargetBitmap nas = new RenderTargetBitmap((int)NodeRender.RenderSize.Width,
            (int)NodeRender.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            nas.Render(NodeRender);
            var cropNode = new CroppedBitmap(nas, new Int32Rect(0, 0, (int)NodeRender.RenderSize.Width, (int)NodeRender.RenderSize.Height));

            System.Drawing.Bitmap weightBitmap;
            System.Drawing.Bitmap edgeBitmap;
            System.Drawing.Bitmap nodeBitmap;


            BitmapEncoder weightEncoder = new PngBitmapEncoder();
            weightEncoder.Frames.Add(BitmapFrame.Create(crop));

            BitmapEncoder edgeEncoder = new PngBitmapEncoder();
            edgeEncoder.Frames.Add(BitmapFrame.Create(cropEdge));

            BitmapEncoder nodeEncoder = new PngBitmapEncoder();
            nodeEncoder.Frames.Add(BitmapFrame.Create(cropNode));

            using (Stream s = new MemoryStream())
            {
                weightEncoder.Save(s);
                weightBitmap = new System.Drawing.Bitmap(s);
            }
            using (Stream s = new MemoryStream())
            {
                edgeEncoder.Save(s);
                edgeBitmap = new System.Drawing.Bitmap(s);
            }
            using (Stream s = new MemoryStream())
            {
                nodeEncoder.Save(s);
                nodeBitmap = new System.Drawing.Bitmap(s);
            }

            var finalImage = new System.Drawing.Bitmap((int)EdgeRender.RenderSize.Width, (int)EdgeRender.RenderSize.Height, PixelFormat.Format32bppArgb);

            var graphics = System.Drawing.Graphics.FromImage(finalImage);
            graphics.CompositingMode = CompositingMode.SourceOver;

            graphics.DrawImage(edgeBitmap, 0, 0);
            graphics.DrawImage(weightBitmap, 0, 0);
            graphics.DrawImage(nodeBitmap, 0, 0);

            return finalImage;
        }

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

        //Events
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
            GetImage().Save(@"./logo.png");
        }
    }
}
