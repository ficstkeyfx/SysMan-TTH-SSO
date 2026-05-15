using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SysMan.Libs
{
    public class XuLyFileExcel
    {
        public void CreateExcelFile(string filePath)
        {
            // Tạo spreadsheet theo filePath
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                // Thêm workbook vào documment
                WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // Thêm worksheet vào workbook
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Thêm Sheet vào qua collection
                Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet
                {
                    Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Sheet 1"
                };
                sheets.Append(sheet);

                // Truy đến worksheet để thêm các hàng và cột
                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Tạo 1 instance hàng
                Row row = new Row();

                // Thêm các ô vào hàng 
                Cell cell1 = new Cell()
                {
                    CellReference = "A1",
                    DataType = CellValues.String,
                    CellValue = new CellValue("ABCC của ô 1")
                };
                Cell cell2 = new Cell()
                {
                    CellReference = "B1", 
                    DataType = CellValues.String,
                    CellValue = new CellValue("CDEE của ô 2")
                };

                // Thêm các ô vào hàng
                row.Append(cell1, cell2);

                // Thêm hàng vào Sheet
                sheetData.Append(row);
                // Tạo 1 instance hàng
                Row row2 = new Row();

                // Thêm các ô vào hàng 
                Cell cell21 = new Cell()
                {
                    CellReference = "A2",
                    DataType = CellValues.String,
                    CellValue = new CellValue("ABCC của ô 1")
                };
                Cell cell22 = new Cell()
                {
                    CellReference = "B2", 
                    DataType = CellValues.String,
                    CellValue = new CellValue("CDEE của ô 2")
                };

                // Thêm các ô vào hàng
                row.Append(cell21, cell22);

                // Thêm hàng vào Sheet
                sheetData.Append(row2);

                // Lưu workbook
                workbookPart.Workbook.Save();
            }
        }
    }
}