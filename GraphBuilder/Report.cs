using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using System;

namespace GraphBuilder
{
    public class Report
    {
        /// <summary>
        /// Тело документа
        /// </summary>
        private Body body;


        public class image
        {
            public string fileName;
            public double width;
            public double height;
            public image (string _fileName, double _width, double _height)
            {
                this.fileName = _fileName;
                this.width = _width;
                this.height = _height;
            }
        }
        public List<image> images = new List<image>();
        public Report()
        {
            // Инициализация тела документа
            this.body = new Body();
        }

        #region Методы манипуляции с документом
        /// <summary>
        /// Добавить параграф в документ
        /// </summary>
        /// <param name="content">Содержание параграфа</param>
        public void AddParagraph(string content)
        {
            // Получение параграфа с заданным содержанием
            Paragraph paragraph = this.GetParagraph(content);

            // Добавление параграфа в тело документа
            this.body.Append(paragraph);
        }
        /// <summary>
        /// Добавить таблицу в документ
        /// </summary>
        /// <param name="content">Таблица, добавляемая в документ</param>
        public void AddTable(List<string[,]> content)
        {
            // Количество строк таблицы
            int rowsCount = content.Count;
            // Количество столбцов таблицы
            int columnsCount = content.First().Length;
            Table table = new Table();

            // Границы таблицы
            TableProperties borders = this.GetTableBorder();
            table.Append(borders);

            for (int i = 0; i < rowsCount; i++)
            {
                TableRow row = new TableRow();

                for (int j = 0; j < columnsCount / 2; j++)
                {
                    TableCell cell = new TableCell();


                    TableCellProperties tcp = new TableCellProperties(
                        new TableCellWidth { Width = Convert.ToString(AdjencyMatrix.ITEM_SIZE), Type = TableWidthUnitValues.Pct }
                    );

                    if (content[i][j, 1] != null)
                        tcp.Append(new Shading() { Color = "auto", Fill = "98EA98", Val = ShadingPatternValues.Clear });
                    else if (i == 0 || j == 0)
                        tcp.Append(new Shading() { Color = "auto", Fill = "FFE599", Val = ShadingPatternValues.Clear });

                    cell.Append(tcp);
                    // Значение в ячейке
                    Paragraph value = this.GetParagraph(content[i][j,0]);

                    // Добавление значения в ячейку
                    cell.Append(value);
                    // Добавление ячейки в строку
                    row.Append(cell);
                }

                // Добавление строки в таблицу
                table.Append(row);
            }

            this.body.Append(table);
        }

        public void AddImageToBody(string imageFileName, double width, double height)
        {
            images.Add(new image(imageFileName, width, height)); 
        }
        private static Drawing GetImageElement(
            string imagePartId,
            string fileName,
            string pictureName,
            double width,
            double height)
        {
            double englishMetricUnitsPerInch = 914400;
            double pixelsPerInch = 96;

            //calculate size in emu
            double emuWidth = (width * englishMetricUnitsPerInch / pixelsPerInch) * .6;
            double emuHeight = (height * englishMetricUnitsPerInch / pixelsPerInch) * .6;

            var element = new Drawing(
                new DW.Inline(
                    new DW.Extent { Cx = (Int64Value)emuWidth, Cy = (Int64Value)emuHeight },
                    new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties { Id = (UInt32Value)1U, Name = pictureName },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                    new A.GraphicFrameLocks { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties { Id = (UInt32Value)0U, Name = fileName },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip(
                                        new A.BlipExtensionList(
                                            new A.BlipExtension { Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}" }))
                                    {
                                        Embed = imagePartId,
                                        CompressionState = A.BlipCompressionValues.Print
                                    },
                                            new A.Stretch(new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset { X = 0L, Y = 0L },
                                        new A.Extents { Cx = (Int64Value)emuWidth, Cy = (Int64Value)emuHeight }),
                                    new A.PresetGeometry(
                                        new A.AdjustValueList())
                                    { Preset = A.ShapeTypeValues.Rectangle })))
                        {
                            Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"
                        }))
                {
                    DistanceFromTop = 0U,
                    DistanceFromBottom = 0U,
                    DistanceFromLeft = 0U,
                    DistanceFromRight = 0U,
                    EditId = "50D07946"
                });
            return element;
        }
        #endregion

        /// <summary>
        /// Получение параграфа с данным содержанием
        /// </summary>
        /// <param name="content">Содержание параграфа</param>
        /// <returns></returns>
        private Paragraph GetParagraph(string content)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            Text text = new Text(content);

            // Добавление текста в набор
            run.Append(text);
            // Добавление набора текста в параграф
            paragraph.Append(run);

            return paragraph;
        }
        /// <summary>
        /// Получение свойств с границами для таблицы
        /// </summary>
        /// <param name="color">Цвет границ (по умолчанию чёрный)</param>
        /// <returns></returns>
        private TableProperties GetTableBorder(string color = "#000000")
        {
            // Свойства
            TableProperties properties = new TableProperties();

            // Границы
            TableBorders borders = new TableBorders();
            #region Задание границ
            // Добавление левой границы
            borders.Append(new LeftBorder()
            {
                Val = new EnumValue<BorderValues>(BorderValues.Thick),
                Color = "#000000",
            });
            // Добавление верхней границы
            borders.Append(new TopBorder()
            {
                Val = new EnumValue<BorderValues>(BorderValues.Thick),
                Color = "#000000",
            });
            // Добавление правой границы
            borders.Append(new RightBorder()
            {
                Val = new EnumValue<BorderValues>(BorderValues.Thick),
                Color = "#000000",
            });
            // Добавление нижней границы
            borders.Append(new BottomBorder()
            {
                Val = new EnumValue<BorderValues>(BorderValues.Thick),
                Color = "#000000",
            });
            // Добавление внутренних горихонтальных границ
            borders.Append(new InsideHorizontalBorder()
            {
                Val = new EnumValue<BorderValues>(BorderValues.Thick),
                Color = "#000000",
            });
            // Добавление внутренних вертикальных границ
            borders.Append(new InsideVerticalBorder()
            {
                Val = new EnumValue<BorderValues>(BorderValues.Thick),
                Color = "#000000",
            });
            #endregion

            properties.Append(borders);


            return properties;
        }

        /// <summary>
        /// Сохранить документ по указанном пути
        /// </summary>
        /// <param name="path">Путь, по которому нужно сохранить документ</param>
        public void SaveDocument(string path)
        {
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();

                foreach (image item in images)
                {
                    ImagePart imagePart = wordDocument.MainDocumentPart.AddImagePart(ImagePartType.Png);

                    using (FileStream stream = new FileStream(item.fileName, FileMode.Open))
                    {
                        imagePart.FeedData(stream);
                    }

                    this.body.AppendChild(new Paragraph(new Run(GetImageElement(
                         wordDocument.MainDocumentPart.GetIdOfPart(imagePart),
                         item.fileName, item.fileName, item.width, item.height))));
                }

                mainPart.Document = new Document();
                mainPart.Document.Body = this.body;
                mainPart.Document.Save();
                wordDocument.Save();
            }
        }
    }
}