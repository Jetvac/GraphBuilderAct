using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using static GraphBuilder.Graph;

namespace GraphBuilder
{
    static class GraphicWorks
    {
        //Base element size
        const int WIDTH = 40;
        const int HEIGHT = 40;

        /// <summary>
        /// Initialize node, set basic dependencies and place it on canvas
        /// </summary>
        /// <returns>Created node</returns>
        public static Node InitializeNode(Canvas nodeGraphicLayer, string name, double posX, double posY)
        {
            Node created = new Node(name, posX, posY);

            //Create label object and add basic event dependencies
            created.VisualAdapter = new Label()
            { Style = (Style)MainWindow.AppResources["Node"], Width = WIDTH, Height = HEIGHT, Content = name };
            created.VisualAdapter.MouseUp += MainWindow.Node_MouseUp;
            created.VisualAdapter.MouseDown += MainWindow.Node_MouseDown;
            created.VisualAdapter.MouseMove += MainWindow.Node_MouseMove;

            //Place label on canvas
            PlaceNode(nodeGraphicLayer, created);

            return created;
        }

        /// <summary>
        /// Initialize and add edge to node, set basic dependencies and place it on canvas
        /// </summary>
        /// <param name="edgeGraphicLayer"></param>
        /// <param name="baseNode"></param>
        /// <param name="addressNode"></param>
        /// <returns></returns>
        public static Edge AddEdge(this Node baseNode, Canvas edgeGraphicLayer, Node addressNode)
        {
            Edge created = new Edge(baseNode, addressNode);
            created.VisualAdapter = new Line() { Style = (Style)MainWindow.AppResources["Edge"] };

            //Place edge on canvas
            PlaceEdge(edgeGraphicLayer, created, baseNode, addressNode);

            return created;
        }


        /// <summary>
        /// Change node position on canvas
        /// </summary>
        /// <param name="node"></param>
        /// <param name="newPosX"></param>
        /// <param name="newPosY"></param>
        public static void MoveNode(this Node node, double newPosX, double newPosY)
        {
            node.PosX = newPosX;
            node.PosY = newPosY;

            //Move node on canvas
            Canvas.SetLeft(node.VisualAdapter, newPosX);
            Canvas.SetTop(node.VisualAdapter, newPosY);

            //Move edge on canvas (First Position)
            foreach (Edge edge in node.baseEdges)
            {
                edge.MoveFirstPointOfEdge(newPosX, newPosY);
            }

            //Move edge on canvas (Second Position)
            foreach (Edge edge in node.addressEdges)
            {
                edge.MoveSecondPointOfEdge(newPosX, newPosY);
            }
        }

        /// <summary>
        /// Change position of first point of edge on canvas
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="newPosX"></param>
        /// <param name="newPosY"></param>
        private static void MoveFirstPointOfEdge(this Edge edge, double newPosX, double newPosY)
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
        private static void MoveSecondPointOfEdge(this Edge edge, double newPosX, double newPosY)
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
        private static bool PlaceNode(Canvas nodeGraphicLayer, Node node)
        {
            //check data
            if (node == null) return false;
            if (nodeGraphicLayer.Children.Contains(node.VisualAdapter)) { nodeGraphicLayer.Children.Remove(node.VisualAdapter);}

            nodeGraphicLayer.Children.Add(node.VisualAdapter);
            Canvas.SetLeft(node.VisualAdapter, node.PosX);
            Canvas.SetTop(node.VisualAdapter, node.PosY);

            return true;
        }

        /// <summary>
        /// Place edge on canvas
        /// </summary>
        /// <param name="edgeGraphicLayer"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        private static bool PlaceEdge(Canvas edgeGraphicLayer, Edge edge, Node baseNode, Node addressNode)
        {
            //check data
            if (edge == null || baseNode == null || addressNode == null) return false;

            MoveFirstPointOfEdge(edge, baseNode.PosX, baseNode.PosY);
            MoveSecondPointOfEdge(edge, addressNode.PosX, addressNode.PosY);
            edgeGraphicLayer.Children.Add(edge.VisualAdapter);

            return true;
        }
    }
}
