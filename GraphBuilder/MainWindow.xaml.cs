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
        public static Graph Graph;
        public static DBController dbController { get; set; }
        public static LP04Entities dbConnection { get; set; } = new LP04Entities();
        public static GraphicInterface GraphicController { get; set; }
        public static ResourceDictionary AppResources { get; private set; }
        private static ListView projectList { get; set; }

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

            projectList = GraphProject;
            UpdateProjectList();
        }

        public static void UpdateProjectList()
        {
            projectList.ItemsSource = dbConnection.GraphStructure.ToList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
            if (GraphicController == null) { MessageBox.Show("Не выбран проект.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information); return; }

            switch (GraphicController.CurrentUserControlType)
            {
                case UserInputController.NodeCreating:
                    Point mousePos = e.GetPosition((Border)sender);

                    Node node = dbController.CreateNode("F1", mousePos.X, mousePos.Y);
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
            dbController = null;
            GraphStructure structure = GraphProject.SelectedItem as GraphStructure;
            GraphName.Text = structure.Name;

            dbController = new DBController(structure.Name, structure);
            Graph = dbController.GetGraph();

            GraphicController = new GraphicInterface(EdgeRender, NodeRender, WeightRender, ref Graph);
        }
        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            dbController = new DBController("Новый проект");
            Graph = dbController.GetGraph();
            GraphName.Text = "Новый проект";

            GraphicController = new GraphicInterface(EdgeRender, NodeRender, WeightRender, ref Graph);

            UpdateProjectList();
        }
        private void GraphName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dbController == null) { return; }
            dbController.ChangeGraphName(GraphName.Text);

            UpdateProjectList();
        }
    }
}
