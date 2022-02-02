using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static GraphBuilder.Graph;

namespace GraphBuilder
{
    public class DBController
    {
        private Graph _graphRef { get; set; }
        private LP04Entities _lpConnection { get; set; } = new LP04Entities();
        private GraphStructure _dbGraph { get; set; }
        // Define an ammount of created node and edges by seance

        public DBController(string name, GraphStructure usingGraphSpace = null)
        {
            if (usingGraphSpace != null) { _dbGraph = usingGraphSpace; }
            else {
                int graphID = _lpConnection.GraphStructure.Count() + 1;
                _dbGraph = new GraphStructure() { GraphID = graphID, Name = name };
                _lpConnection.GraphStructure.Add(_dbGraph);
            }

            InitializeGraph();
        }

        /// <summary>
        /// Create base program structure by DB data
        /// </summary>
        private void InitializeGraph()
        {
            _graphRef = new Graph();

            foreach (GraphNode node in _dbGraph.GraphNode)
            {
                _graphRef.CreateBaseNode(node.NodeID, node.Name, node.PosX, node.PosY);
            }

            foreach (GraphEdge edge in _dbGraph.GraphEdge)
            {
                Node baseNode = _graphRef.FindNodeByID(edge.BaseNodeID);
                Node adressNode = _graphRef.FindNodeByID(edge.AddressNodeID);

                _graphRef.CreateBaseEdge(edge.EdgeID, baseNode, adressNode, edge.Weight);
            }
        }


        // Database manipulate methods
        public void ChangeGraphName(string newName)
        {
            _dbGraph.Name = newName;
            SaveChanges();
        }
        /// <summary>
        /// Create new node in database and return result
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="nodeName"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        public Node CreateNode(string nodeName, double posX, double posY)
        {
            // Create node in base
            int newNodeID = _lpConnection.GraphNode.Count() + 1;

            GraphNode newNode = new GraphNode { GraphID = _dbGraph.GraphID, NodeID = newNodeID, Name = nodeName, PosX = posX, PosY = posY, Abbreviation = "" };
            _dbGraph.GraphNode.Add(newNode);
            _lpConnection.GraphNode.Add(newNode);

            SaveChanges();

            // Initialize logic element
            Node node = _graphRef.CreateBaseNode(newNode.NodeID, nodeName, posX, posY);
            return node;
        }
        /// <summary>
        /// Create new edge in database and return result
        /// </summary>
        /// <param name="baseNode"></param>
        /// <param name="adressNode"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public Edge CreateEdge(Node baseNode, Node adressNode, int weight)
        {
            // Create edge in base
            int newEdgeID = _lpConnection.GraphEdge.Count() + 1;

            GraphEdge newEdge = new GraphEdge { GraphID = _dbGraph.GraphID, EdgeID = newEdgeID, BaseNodeID = baseNode.ID, AddressNodeID = adressNode.ID };
            _dbGraph.GraphEdge.Add(newEdge);
            _lpConnection.GraphEdge.Add(newEdge);

            SaveChanges();

            // Initialize logic element
            Edge edge = _graphRef.CreateBaseEdge(newEdge.EdgeID, baseNode, adressNode, weight);
            return edge;
        }
        /// <summary>
        /// Remove node and related edge from database
        /// </summary>
        /// <param name="node"></param>
        public void DeleteNode(Node node)
        {
            GraphNode graphNode = _dbGraph.GraphNode.FirstOrDefault(c => c.NodeID == node.ID);
            List<GraphEdge> relatedEdge = _dbGraph.GraphEdge.Where(c => c.BaseNodeID == graphNode.NodeID || c.AddressNodeID == graphNode.NodeID).ToList();


            // Почему-то не удаляет (Выдаёт ошибку)
            foreach (GraphEdge edge in relatedEdge) { _lpConnection.GraphEdge.Remove(edge); _dbGraph.GraphEdge.Remove(edge); }
            _lpConnection.GraphNode.Remove(graphNode);
            _dbGraph.GraphNode.Remove(graphNode);
            
            SaveChanges();
        }
        /// <summary>
        /// Change edge weight value in database
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="value"></param>
        public void ChangeEdgeWeightValue(Edge edge, int value)
        {
            GraphEdge data = _dbGraph.GraphEdge.FirstOrDefault(c => c.EdgeID == edge.ID);
            data.Weight = value;
        }
        public void MoveNode(Node node, double newPosX, double newPosY)
        {
            // Позиция не изменяется
            GraphNode moved = _lpConnection.GraphNode.FirstOrDefault(c => c.NodeID == node.ID);
            moved.PosX = newPosX;
            moved.PosY = newPosY;
            SaveChanges();
        }

        // Base contact methods
        /// <summary>
        /// Save changes in data base
        /// </summary>
        private void SaveChanges()
        {
            try
            {
                _lpConnection.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка в процессе сохранения.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Access methods
        public Graph GetGraph()
        {
            return _graphRef;
        }
    }
}
