using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GraphBuilder
{
    public class AdjencyMatrix
    {
        public const int ITEM_SIZE = 40;

        public class HeaderItem
        {
            public GraphNode data { get; set; }
            public TextBox visualizeAdapter { get; set; }
            private int _posX;
            private int _posY;
            public int PosX { get { return _posX; } 
                set
                {
                    if (value < 0) { return; }
                    Grid.SetColumn(visualizeAdapter, value);
                    _posX = value;
                }
            }
            public int PosY { get { return _posY; } 
                set
                {
                    if (value < 0) { return; }
                    Grid.SetRow(visualizeAdapter, value);
                    _posY = value;
                }
            }

            public HeaderItem(GraphNode node, TextBox adapter, int posX, int posY)
            {
                visualizeAdapter = adapter;
                data = node;
                PosX = posX;
                PosY = posY;
            }
        }
        public class CellItem
        {
            public GraphEdge data { get; set;}
            public TextBox visualizeAdapter { get; set; }
            private int _posX;
            private int _posY;
            public int PosX
            {
                get { return _posX; }
                set
                {
                    if (value < 0) { return; }
                    Grid.SetColumn(visualizeAdapter, value);
                    _posX = value;
                }
            }
            public int PosY
            {
                get { return _posY; }
                set
                {
                    if (value < 0) { return; }
                    Grid.SetRow(visualizeAdapter, value);
                    _posY = value;
                }
            }


            public CellItem(GraphEdge edge, TextBox adapter, int posX, int posY)
            {
                visualizeAdapter= adapter;
                data = edge;
                PosX = posX;
                PosY = posY;
            }
        }

        public Grid Matrix { get; set; }
        public List<HeaderItem> HeaderItemsColumns { get; set; } = new List<HeaderItem>();
        public List<HeaderItem> HeaderItemsRows { get; set; } = new List<HeaderItem>();
        public List<CellItem> CellItems { get; set; } = new List<CellItem>();
        public int MatrixSize { get { return MatrixSize; } set { if (value >= 0) { MatrixSize = value; } } }
        public GraphStructure GraphData { get; set; }

        public HeaderItem InitNewHeaderItem(GraphNode node, int posX, int posY)
        {
            TextBox textBoxColumn = new TextBox()
            { IsEnabled = false, Text = node.Abbreviation, Style = (Style)MainWindow.AppResources["GridHeader"], Width = ITEM_SIZE, Height = ITEM_SIZE };

            Matrix.Children.Add(textBoxColumn);
            HeaderItem item = new HeaderItem(node, textBoxColumn, posX, posY);

            return item;
        }
        public CellItem InitNewCellItem(GraphEdge edge, int posX, int posY)
        {
            TextBox textBoxColumn = new TextBox()
            { IsEnabled = false, Text = Convert.ToString(edge.Weight), Style = (Style)MainWindow.AppResources["GridItem"], Width = ITEM_SIZE, Height = ITEM_SIZE };

            Matrix.Children.Add(textBoxColumn);
            CellItem item = new CellItem(edge, textBoxColumn, posX, posY);

            return item;
        }

        public int GetPosByHeader(GraphNode graph)
        {
            return HeaderItemsColumns.FirstOrDefault(c => c.data == graph).PosX;
        }

        public AdjencyMatrix(GraphStructure graph, Grid visual)
        {
            Matrix = visual;
            GraphData = new LP04Entities().GraphStructure.FirstOrDefault(c => c.GraphID == graph.GraphID);

            Matrix.Children.Clear();
            Matrix.RowDefinitions.Clear();
            Matrix.ColumnDefinitions.Clear();

            InitializeGrid();
        }

        public void InitializeGrid()
        {
            int freeCell = 1;

            Matrix.ShowGridLines = true;

            // First cell
            Matrix.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(ITEM_SIZE, GridUnitType.Pixel) });
            Matrix.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(ITEM_SIZE, GridUnitType.Pixel) });


            // Initialize Header rows and columns
            foreach (GraphNode itemData in GraphData.GraphNode)
            {
                Matrix.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                Matrix.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                HeaderItem headerItemColumns = InitNewHeaderItem(itemData, freeCell, 0);
                HeaderItemsColumns.Add(headerItemColumns);
                HeaderItem headerItemRows = InitNewHeaderItem(itemData, 0, freeCell);
                HeaderItemsRows.Add(headerItemRows);

                freeCell++;
            }
            
            // Initialize cells
            foreach (GraphEdge itemData in GraphData.GraphEdge)
            {
                int FirstPos = GetPosByHeader(itemData.GraphNode);
                int LastPos = GetPosByHeader(itemData.GraphNode1);


                CellItem itemCell = InitNewCellItem(itemData, FirstPos, LastPos);
                CellItem itemCellMirrored = InitNewCellItem(itemData, LastPos, FirstPos);

                CellItems.Add(itemCell);
                CellItems.Add(itemCellMirrored);
            }
        }
    }
}
