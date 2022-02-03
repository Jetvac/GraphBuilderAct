using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        public int _structureID { get; set; }
        // Define an ammount of created node and edges by seance

        public DBController(string name, GraphStructure usingGraphSpace = null)
        {
            if (usingGraphSpace != null) { _structureID = usingGraphSpace.GraphID; }
            else {
                int graphID = 0;
                if (!(_lpConnection.GraphStructure.Count() == 0))
                {
                    graphID = _lpConnection.GraphStructure.Select(c => c.GraphID).Max() + 1;
                }
                _structureID = graphID;
                GraphStructure newGraph = new GraphStructure() { GraphID = graphID, Name = name };
                _lpConnection.GraphStructure.Add(newGraph);
                SaveChanges();
            }

            InitializeGraph();
        }

        /// <summary>
        /// Create base program structure by DB data
        /// </summary>
        private void InitializeGraph()
        {
            _graphRef = new Graph();
            GraphStructure graphStructure = _lpConnection.GraphStructure.FirstOrDefault(c => c.GraphID == _structureID);

            foreach (GraphNode node in graphStructure.GraphNode)
            {
                _graphRef.CreateBaseNode(node.NodeID, node.Name, node.PosX, node.PosY);
            }

            foreach (GraphEdge edge in graphStructure.GraphEdge)
            {
                Node baseNode = _graphRef.FindNodeByID(edge.BaseNodeID);
                Node adressNode = _graphRef.FindNodeByID(edge.AddressNodeID);

                _graphRef.CreateBaseEdge(edge.EdgeID, baseNode, adressNode, edge.Weight);
            }
        }


        // Database manipulate methods
        public void ChangeGraphName(string newName)
        {
            _lpConnection.GraphStructure.FirstOrDefault(c => c.GraphID == _structureID).Name = newName;
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
            int newNodeID = 0;
            if (!(_lpConnection.GraphNode.Count() == 0))
            {
                newNodeID = _lpConnection.GraphNode.Select(c => c.NodeID).Max() + 1;
            }


                GraphNode newNode = new GraphNode { GraphID = _structureID, NodeID = newNodeID, Name = nodeName, PosX = posX, PosY = posY, Abbreviation = "" };
            _lpConnection.GraphNode.Add(newNode);

            if (SaveChanges())
            {
                // Initialize logic element
                Node node = _graphRef.CreateBaseNode(newNode.NodeID, nodeName, posX, posY);
                return node;
            }

            return null;
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
            int newEdgeID = 0;
            if (!(_lpConnection.GraphEdge.Count() == 0))
            {
                newEdgeID = _lpConnection.GraphEdge.Select(c => c.EdgeID).Max() + 1;
            }

            GraphEdge newEdge = new GraphEdge { GraphID = _structureID, EdgeID = newEdgeID, BaseNodeID = baseNode.ID, AddressNodeID = adressNode.ID };
            _lpConnection.GraphEdge.Add(newEdge);

            if (SaveChanges())
            {
                // Initialize logic element
                Edge edge = _graphRef.CreateBaseEdge(newEdge.EdgeID, baseNode, adressNode, weight);
                return edge;
            }

            return null;
        }
        /// <summary>
        /// Remove node and related edge from database
        /// </summary>
        /// <param name="node"></param>
        public void DeleteNode(Node node)
        {
            GraphNode graphNode = _lpConnection.GraphNode.FirstOrDefault(c => c.NodeID == node.ID);
            List<GraphEdge> relatedEdge = _lpConnection.GraphEdge.Where(c => c.BaseNodeID == graphNode.NodeID || c.AddressNodeID == graphNode.NodeID).ToList();

            foreach (GraphEdge edge in relatedEdge) { _lpConnection.GraphEdge.Remove(edge); }
            _lpConnection.GraphNode.Remove(graphNode);
            
            SaveChanges();
        }
        /// <summary>
        /// Change edge weight value in database
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="value"></param>
        public void ChangeEdgeWeightValue(Edge edge, int value)
        {
            GraphEdge data = _lpConnection.GraphEdge.FirstOrDefault(c => c.EdgeID == edge.ID);
            data.Weight = value;
            SaveChanges();
        }
        public void MoveNode(Node node, double newPosX, double newPosY)
        {
            GraphNode moved = _lpConnection.GraphNode.FirstOrDefault(c => c.NodeID == node.ID);
            moved.PosX = newPosX;
            moved.PosY = newPosY;
            SaveChanges();
        }
        public bool DeleteProject()
        {
            try
            {
                List<GraphEdge> deletedEdges = _lpConnection.GraphEdge.Where(c => c.GraphID == _structureID).ToList();
                List<GraphNode> deletedNodes = _lpConnection.GraphNode.Where(c => c.GraphID == _structureID).ToList();
                foreach (GraphEdge edge in deletedEdges) { _lpConnection.GraphEdge.Remove(edge); }
                foreach (GraphNode node in deletedNodes) { _lpConnection.GraphNode.Remove(node); }

                _lpConnection.GraphStructure.Remove(_lpConnection.GraphStructure.FirstOrDefault(c => c.GraphID == _structureID));
                SaveChanges();
            } catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        // Base contact methods
        /// <summary>
        /// Save changes in data base
        /// </summary>
        private bool SaveChanges()
        {
            try
            {
                _lpConnection.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка в процессе сохранения.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        // Access methods
        public Graph GetGraph()
        {
            return _graphRef;
        }
    }
}
