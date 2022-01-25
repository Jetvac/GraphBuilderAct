using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace GraphBuilder
{
    public class Graph
    {
        private List<Node> graphNodes { get; set; } = new List<Node>();
        private List<Edge> graphEdges { get; set; } = new List<Edge>();


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


        public Node CreateBaseNode(string name, double posX, double posY)
        {
            Node result = new Node(name, posX, posY);

            graphNodes.Add(result);

            return result;
        }
        public Edge CreateBaseEdge(Node baseNode, Node addressNode)
        {
            Edge result = new Edge(baseNode, addressNode);

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
        /// <summary>
        /// Search edge in graph list with same visualAdapter signature
        /// </summary>
        /// <param name="visualAdapter"></param>
        /// <returns>Edge item</returns>
        public Edge FindEdgeByAdapter(Line visualAdapter)
        {
            return graphEdges.Where(c => c.VisualAdapter.Equals(visualAdapter)).First();
        }
        public Edge SearchEdgeByNodes(Node baseNode, Node addressNode)
        {
            return graphEdges.FirstOrDefault(c => c.BaseNode == baseNode && c.AddressNode == addressNode);
        }
        /// <summary>
        /// Return node from list by given position
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Node GetNodeById(int id)
        {
            return graphNodes[id];
        }
        /// <summary>
        /// Return edge from list by given position
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Edge GetEdgeById(int id)
        {
            return graphEdges[id];
        }
    }
}
