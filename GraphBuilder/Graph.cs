using System.Collections.Generic;
using System.Linq;
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
            public Edge(int id, Node baseNode, Node addressNode, int weight)
            {
                ID = id;
                BaseNode = baseNode;
                AddressNode = addressNode;
                Weight = weight;

                //Add dependencies to node
                baseNode.baseEdges.Add(this);
                addressNode.addressEdges.Add(this);
            }

            public int ID { get; set; }
            public int Weight { get; set; }
            public Line VisualAdapter { get; set; }
            public Node BaseNode { get; set; }
            public Node AddressNode { get; set; }

            public Weight WeightAdapter { get; set; }
        }
        public class Node
        {
            public Node(int id, string name, double posX, double posY, string abbreviation)
            {
                ID = id;
                PosX = posX;
                PosY = posY;
                Name = name;
                Abbreviation = abbreviation;
            }

            public int ID { get; set; }
            public string Name { get; set; }
            public Label VisualAdapter { get; set; }
            public double PosX { get; set; }
            public double PosY { get; set; }
            public string Abbreviation { get; set; }

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


        public Node CreateBaseNode(int id, string name, double posX, double posY, string abbreviation)
        {
            Node result = new Node(id, name, posX, posY, abbreviation);

            graphNodes.Add(result);

            return result;
        }
        
        public Edge CreateBaseEdge(int id, Node baseNode, Node addressNode, int weight)
        {
            Edge result = new Edge(id, baseNode, addressNode, weight);

            graphEdges.Add(result);

            return result;
        }
        /// <summary>
        /// Find all references on node and delete it
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<Edge> RemoveNode(Node node)
        {
            List<Edge> removedEdges = graphEdges.Where(c => c.BaseNode == node || c.AddressNode == node).ToList();
            foreach (Edge item in removedEdges)
                graphEdges.Remove(item);
            graphNodes.Remove(node);

            return removedEdges;
        }
        public void RemoveEdge(Edge edge)
        {
            graphEdges.Remove(edge);
        }

        // Data search
        public Node FindNodeByID(int ID)
        {
            return graphNodes.Find(c => c.ID == ID);
        }
        public Edge FindEdgeByID(int ID)
        {
            return graphEdges.Find(c => c.ID == ID);
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
        /// <summary>
        /// Search edge by weight adapter
        /// </summary>
        /// <param name="visualAdapter"></param>
        /// <returns></returns>
        public Edge FindEdgeByWeightAdapter(TextBox visualAdapter)
        {
            return graphEdges.FirstOrDefault(c => c.WeightAdapter.VisualAdapter == visualAdapter);
        }
        public Edge SearchEdgeByNodes(Node baseNode, Node addressNode)
        {
            return graphEdges.FirstOrDefault(c => c.BaseNode == baseNode && c.AddressNode == addressNode);
        }
        /// <summary>
        /// Search edge by two point (without direction)
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public Edge SearchEdgeNonStraightedByNodes(Node first, Node second)
        {
            return graphEdges.FirstOrDefault(c => (c.BaseNode == first && c.AddressNode == second) || (c.BaseNode == second && c.AddressNode == first));
        }
    }
}
