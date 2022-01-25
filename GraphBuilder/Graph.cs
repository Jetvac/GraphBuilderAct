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
        public List<Node> graphNodes { get; set; } = new List<Node>();
        public List<Edge> graphEdges { get; set; } = new List<Edge>();


        public class Edge 
        {
            public Edge(Node baseNode, Node addressNode)
            {
                BaseNode = baseNode;
                AddressNode = addressNode;

                //Add dependencies to node
                baseNode.baseEdges.Add(this);
                addressNode.addressEdges.Add(this);
            }

            public Line VisualAdapter { get; set; }
            public Node BaseNode { get; set; }
            public Node AddressNode { get; set; }

            public double Value { get; set; }
        }
        public class Node
        {
            public Node(string name, double posX, double posY)
            {
                PosX = posX;
                PosY = posY;
                Name = name;
            }
            public string Name { get; set; }
            public Label VisualAdapter { get; set; }
            public double PosX { get; set; }
            public double PosY { get; set; }

            public List<Edge> baseEdges { get; set; } = new List<Edge>();
            public List<Edge> addressEdges { get; set; } = new List<Edge>();
        }

        

        /// <summary>
        /// Search node in graph list with same visualAdapter signature
        /// </summary>
        /// <param name="visualAdapter"></param>
        /// <returns>Node item</returns>
        public Node FindNodeByAdapter(Label visualAdapter)
        {
            return graphNodes.Where(c => c.VisualAdapter.Equals(visualAdapter)).First();
        }

        /// <summary>
        /// Search edge in graph list with same visualAdapter signature
        /// </summary>
        /// <param name="visualAdapter"></param>
        /// <returns>Edge item</returns>
        public Edge FindEdgeByAdapter(Line visualAdapter)
        {
            return graphEdges.Where(c => c.VisualAdapter.Equals(visualAdapter)).First();
        }
    }
}
