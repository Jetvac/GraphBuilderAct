using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace GraphBuilder
{
    public class Graph
    {
        private List<Node> graphNodes { get; set; } = new List<Node>();


        public class Edge 
        {
            public Edge(Weight weightAdapter)
            {
                WeightAdapter = weightAdapter;
            }

            public Line VisualAdapter { get; set; }
            public Weight WeightAdapter { get; set; }
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
        public class Weight
        {
            public double Value { get; set; }
            public TextBox VisualAdapter { get; set; }

            public Weight(double value, TextBox visual)
            {
                Value = value;
                VisualAdapter = visual;
            }
        }


        public Node CreateBaseNode(string name, double posX, double posY)
        {
            Node result = new Node(name, posX, posY);

            graphNodes.Add(result);

            return result;
        }
        public Edge CreateBaseEdge(Node baseNode, Node addressNode, Weight weightAdapter)
        {
            Edge result = new Edge(weightAdapter);

            baseNode.baseEdges.Add(result);
            addressNode.addressEdges.Add(result);

            return result;
        }
        /// <summary>
        /// Find all references on node and delete it
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<Edge> RemoveNode(Node node)
        {
            List<Edge> removedEdges = node.baseEdges; removedEdges.AddRange(node.addressEdges);
            graphNodes.Remove(node);

            return removedEdges;
        }
        public Edge FindEdgeByWeightAdapter(TextBox weight)
        {
            foreach(Node node in graphNodes)
            {
                Edge edge = node.baseEdges.FirstOrDefault(c => c.WeightAdapter.VisualAdapter == weight);
                return edge;
            }
            return null;
        }
        /// <summary>
        /// Search node in baseEdges list 
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public Node FindBaseNodeByEdge(Edge edge)
        {
            return graphNodes.FirstOrDefault(c => c.baseEdges.Contains(edge));
        }
        /// <summary>
        /// Search node in addressEdges list
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public Node FindAdressNodeByEdge(Edge edge)
        {
            return graphNodes.FirstOrDefault(c => c.addressEdges.Contains(edge));
        }
        public Edge SearchEdgeByNodes(Node baseNode, Node addressNode)
        {
            foreach (Edge edge in baseNode.baseEdges)
            {
                if (addressNode.addressEdges.Contains(edge))
                {
                    return edge;
                }
            }
            return null;
        }
        public Node SearchNodeByEdgeAndNode(Edge edge, Node baseNode)
        {
            foreach(Node addressNode in graphNodes)
            {
                if (addressNode.baseEdges.Contains(edge) && addressNode != baseNode)
                {
                    return addressNode;
                }
            }
            return null;
        }

        //Get interface
        /// <summary>
        /// Search node in graph list with same visualAdapter signature
        /// </summary>
        /// <param name="visualAdapter"></param>
        /// <returns>Node item</returns>
        public Node FindNodeByAdapter(Label visualAdapter)
        {
            return graphNodes.Where(c => c.VisualAdapter.Equals(visualAdapter)).First();
        }
        public Node GetNodeById(int id)
        {
            return graphNodes[id];
        }
    }
}
