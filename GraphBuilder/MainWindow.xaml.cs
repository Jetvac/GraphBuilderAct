using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        public static GraphicInterface GraphicController { get; set; }
        public static ResourceDictionary AppResources { get; private set; }
        private static ListView _projectList { get; set; }
        private static bool _isListUpdating { get; set; } = false;
        public static ObservableCollection<GraphStructure> projectListSource = new ObservableCollection<GraphStructure>();

        public static void ChangeControlMode(UserInputController type)
        {
            if (GraphicController != null)
            {
                GraphicController.SwitchCurrentControlMode(type);
            } else
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
        }

        public static void UpdateProjectList()
        {
            _isListUpdating = true;
            projectListSource = ListToOBS(dbConnection.GraphStructure.ToList());
            _projectList.ItemsSource = projectListSource;
            _isListUpdating = false;
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
            GraphName.Text = "Не выбран";
            GraphName.IsEnabled = false;
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

                    Node node = dbController.CreateNode("F1", mousePos.X, mousePos.Y);
                    if (node == null) { return; }
                    GraphicController.InitializeNodeGraphic(node);
                    break;
            }
        }
        private void DeleteNodeSwitch_Click(object sender, RoutedEventArgs e)
        {
            ChangeControlMode(UserInputController.NodeDelete);
        }

        // Tab menu
        private void GraphProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isListUpdating) { _isListUpdating = false; return; }
            dbController = null;
            GraphStructure structure = GraphProject.SelectedItem as GraphStructure;
            GraphName.Text = structure.Name;
            GraphName.IsEnabled = true;

            dbController = new DBController(structure.Name, structure);
            classGraph = dbController.GetGraph();

            GraphicController = new GraphicInterface(EdgeRender, NodeRender, WeightRender, ref classGraph);
        }
        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            GraphName.IsEnabled = true;

            // Create an object
            dbController = new DBController("Новый проект");
            classGraph = dbController.GetGraph();
            GraphName.Text = "Новый проект";

            UpdateProjectList();
            GraphStructure created = dbConnection.GraphStructure.FirstOrDefault(c => c.GraphID == dbController._structureID);
            _projectList.SelectedItem = created;
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
            UpdateProjectList();
            GraphName.Text = "Не выбран";
            GraphName.IsEnabled = false;
        }
    }
}
