using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace GraphBuilder
{
    public class Graph
    {
        const int WIDTH = 40;
        const int HEIGHT = 40;

        public class Edge 
        {
            public Edge(Node baseNode, Node addressNode)
            {
                BaseNode = baseNode;
                AddressNode = addressNode;
                VisualAdapter = new Line()
                {
                    Style = (Style)MainWindow.AppResources["Edge"],
                    X1 = BaseNode.PosX + WIDTH / 2, Y1 = BaseNode.PosY + HEIGHT / 2,
                    X2 = AddressNode.PosX + WIDTH / 2, Y2 = AddressNode.PosY + HEIGHT / 2
                };

                baseNode.baseEdges.Add(this);
                addressNode.addressEdges.Add(this);
            }

            public Line VisualAdapter { get; set; }
            public Node BaseNode { get; set; }
            public Node AddressNode { get; set; }

            public double Value { get; set; }

            public void ChangeStartPosition(double newStartX, double newStartY)
            {
                VisualAdapter.X1 = newStartX + WIDTH / 2;
                VisualAdapter.Y1 = newStartY + HEIGHT / 2;
            }

            public void ChangeEndPosition(double newEndX, double newEndY)
            {
                VisualAdapter.X2 = newEndX + WIDTH / 2;
                VisualAdapter.Y2 = newEndY + HEIGHT / 2;
            }
        }
        public class Node
        {
            public Node(double posX, double posY, string name)
            {
                Name = name;
                PosX = posX;
                PosY = posY;
                VisualAdapter = new Label()
                { Style = (Style)MainWindow.AppResources["Node"], Width = WIDTH, Height = HEIGHT, Content = Name };

                VisualAdapter.MouseUp += MainWindow.Node_MouseUp;
                VisualAdapter.MouseDown += MainWindow.Node_MouseDown;
                VisualAdapter.MouseMove += MainWindow.Node_MouseMove;
            }
            public double PosX { get; set; }
            public double PosY { get; set; }
            public string Name { get; set; }
            public Label VisualAdapter { get; set; }

            public List<Edge> baseEdges { get; set; } = new List<Edge>();
            public List<Edge> addressEdges { get; set; } = new List<Edge>();

            public void ChangePosition(double newPosX, double newPosY)
            {
                PosX = newPosX;
                PosY = newPosY;
            }
        }
    }
}
