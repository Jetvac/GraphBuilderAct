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
        public GraphStructure GraphData { get; set; }


        // Init methods
        /// <summary>
        /// Метод инициализации загаловочных ячеек матрицы
        /// </summary>
        /// <param name="node">Источник данных используемый для заполнения ячейки</param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        public HeaderItem InitNewHeaderItem(GraphNode node, int posX, int posY)
        {
            TextBox textBoxColumn = new TextBox()
            { IsEnabled = false, Text = node.Abbreviation, Style = (Style)MainWindow.AppResources["GridHeader"], Width = ITEM_SIZE, Height = ITEM_SIZE };

            Matrix.Children.Add(textBoxColumn);
            HeaderItem item = new HeaderItem(node, textBoxColumn, posX, posY);

            return item;
        }
        /// <summary>
        /// Метод инициализации ячейки в матрице
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public CellItem InitNewCellItem(GraphEdge edge, int posX, int posY, string style)
        {
            TextBox textBoxColumn = new TextBox()
            { IsEnabled = false, Text = Convert.ToString(edge.Weight), Style = (Style)MainWindow.AppResources[style], Width = ITEM_SIZE, Height = ITEM_SIZE };

            Matrix.Children.Add(textBoxColumn);
            CellItem item = new CellItem(edge, textBoxColumn, posX, posY);

            return item;
        }
        /// <summary>
        /// Возвращает строку загаловочной ячейки
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public int GetPosByHeader(GraphNode graph)
        {
            return HeaderItemsColumns.FirstOrDefault(c => c.data == graph).PosX;
        }
        /// <summary>
        /// Генерирует матрицу и заполняет её
        /// </summary>
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

                string style = "GridItem";

                if (MainWindow.GraphicController != null)
                    if (MainWindow.GraphicController._activatedPath.FirstOrDefault(c => c.ID == itemData.EdgeID) != null)
                    {
                        style = "ActivatedGridItem";
                    }

                CellItem itemCell = InitNewCellItem(itemData, FirstPos, LastPos, style);
                CellItem itemCellMirrored = InitNewCellItem(itemData, LastPos, FirstPos, style);

                CellItems.Add(itemCell);
                CellItems.Add(itemCellMirrored);
            }
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

        // Work methods
        public List<string[,]> GetOutputMatrix()
        {
            List<string[,]> output = new List<string[,]>();

            for (int i = 0; i < HeaderItemsRows.Count(); i++)
            {
                output.Add(new string[HeaderItemsRows.Count(), 2]);

                for (int j = 0; j < HeaderItemsColumns.Count(); j++)
                {
                    if (i == 0 && j != 0) // Загаловочная ячейка (Слева)
                    {
                        output[i][j,0] = HeaderItemsRows[j].data.Abbreviation;
                    }
                    else if (j == 0 && i != 0) // Загаловочная ячейка (Сверху)
                    {
                        output[i][j,0] = HeaderItemsColumns[i].data.Abbreviation;
                    }
                    else if (i == 0 && j != 0)
                    {
                        output[i][j,0] = "";
                    }
                    else if (i != 0 && j != 0)
                    {
                        CellItem item = CellItems.FirstOrDefault(c => c.PosX == j && c.PosY == i);
                        if (item != null)
                            if (MainWindow.GraphicController != null)
                                if (MainWindow.GraphicController._activatedPath.FirstOrDefault(c => c.ID == item.data.EdgeID) != null)
                                {
                                    output[i][j, 0] = Convert.ToString(item.data.Weight);
                                    output[i][j, 1] = "#90EE90";
                                } else
                                {
                                    output[i][j, 0] = Convert.ToString(item.data.Weight);
                                }
                        else
                            output[i][j,0] = "";
                    }
                }
            }

            return output;
        }
    }
}
