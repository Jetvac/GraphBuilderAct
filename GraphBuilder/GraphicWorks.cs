using System.Windows;
using System.Windows.Controls;
using static GraphBuilder.Graph;

namespace GraphBuilder
{
    internal class GraphicWorks
    {
        const int WIDTH = 40;
        const int HEIGHT = 40;


        /// <summary>
        /// Create and return base styled node
        /// </summary>
        /// <returns></returns>
        public static Label CreateNode()
        {
            Label result = new Label()
            { Style = (Style)MainWindow.AppResources["Node"], Width = WIDTH, Height = HEIGHT };

            return result;
        }

        /// <summary>
        /// Place node on canvas by given position
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="node"></param>
        /// <param name="pointX"></param>
        /// <param name="pointY"></param>
        /// <returns></returns>
        public static bool PlaceNode(Canvas canvas, Node node)
        {
            canvas.Children.Add(node.VisualAdapter);
            Canvas.SetLeft(node.VisualAdapter, node.PosX);
            Canvas.SetTop(node.VisualAdapter, node.PosY);

            return true;
        }

        /// <summary>
        /// Place edge on canvas by given position
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static bool PlaceEdge(Canvas canvas, Edge edge)
        {
            canvas.Children.Add(edge.VisualAdapter);

            return true;
        }
    }
}
