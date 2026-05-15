using Microsoft.JSInterop;
using System.IO;
using System;
using System.Net.Http.Headers;
using DocumentFormat.OpenXml.Packaging;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Presentation;
namespace SysMan
{
    public enum AcceptStatus
    {
        ChuaGui = 0,
        DaGui = 1,
        DaXacNhan = 2,
        TuChoiXacNhan = 3,
    }

    public enum HistoryAction
    {
        DaXoa = -1,
        DaTao = 0,
        DaGui = 1,
        DaXacNhan = 2,
        TuChoiXacNhan = 3,

    }
    public class TypeDrop
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Files
    {
        private readonly IJSRuntime js;

        public Files(IJSRuntime js)
        {
            this.js = js;
        }

        /// <summary>
        /// Phương thức chuyển đổi file .Docx sang pdf
        /// phương thức không làm việc được với file .Doc
        /// </summary>
        /// <param name="filePath">đường dẫn tương đối đến file .Docx</param>
        /// <returns>Đường dẫn tương đối đến file pdf</returns>
        public static async Task<string> ConvertToPDF(string filePath)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                            .AddJsonFile("appsettings.json")
                            .Build();
            string gotenberg_endpoint = configuration.GetConnectionString("GotenbergEndpoint");
            var httpClient = new HttpClient();
            if (Path.GetFileName(filePath).Contains(".pdf"))
            {
                return filePath;
            }
            string outputFilePath = Path.ChangeExtension(filePath, ".pdf");

            var filesContent = new MultipartFormDataContent();
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            ByteArrayContent fileContent = new ByteArrayContent(fileBytes);
            filesContent.Add(fileContent, "files", Path.GetFileName(filePath));
            var response = await httpClient.PostAsync($"{gotenberg_endpoint}/forms/libreoffice/convert", filesContent);
            if (response.IsSuccessStatusCode)
            {
                byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(outputFilePath, pdfBytes);
                return outputFilePath;
            }
            return "";
        }
        public async void Download(string filePath, string username)
        {
            try
            {
                await js.InvokeAsync<object>("open", $"/{filePath}", "_blank");
            }
            catch { }
        }

        public async Task View(string filePath)
        {
            if (filePath.Contains(".pdf"))
            {
                try
                {

                    await js.InvokeVoidAsync("eval", $"let _discard_ = open(`{filePath}`, `_blank`)");
                }
                catch
                {

                }
            }
            else
            {
                string newfilePath = "";
                if (filePath.ToLower().Contains(".doc") || filePath.ToLower().Contains(".xls"))
                {
                    newfilePath = await ConvertToPDF(filePath);
                }
                else
                {
                    newfilePath = filePath;
                }
                try
                {
                    await js.InvokeVoidAsync("eval", $"let _discard_ = open(`{newfilePath}`, `_blank`)");
                }
                catch
                {

                }
            }
        }
    }
    public static class FormFileHelper
    {
        public static IFormFile GetFormFileFromPath(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                stream.CopyTo(memoryStream);
            }
            memoryStream.Position = 0;

            return new FormFile(memoryStream, 0, memoryStream.Length, fileInfo.Name, fileInfo.Name)
            {
                Headers = new HeaderDictionary(),
                ContentType = GetContentType(filePath)
            };
        }

        private static string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        private static Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                { ".txt", "text/plain" },
                { ".pdf", "application/pdf" },
                { ".doc", "application/vnd.ms-word" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".csv", "text/csv" }
            };
        }
        public static string GetFileType(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName).ToLower();
            if (new[] { ".doc", ".xls", ".ppt", ".txt", ".pdf", ".docx", ".xlsx" }.Contains(fileExtension))
                return "file-van-ban";
            if (new[] { ".mp3", ".wav", ".aac" }.Contains(fileExtension))
                return "file-am-thanh";
            if (new[] { ".mp4", ".avi", ".mkv", ".mov" }.Contains(fileExtension))
                return "file-video";
            if (new[] { ".exe" }.Contains(fileExtension))
                return "file-chuong-trinh";
            if (new[] { ".jpeg", ".jpg", ".png", ".gif" }.Contains(fileExtension))
                return "file-anh";
            return "file-khac";
        }
    }

    public static class PageCounter
    {
        public static int GetPageCount(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension == ".docx")
            {
                return GetWordPageCount(file);
            }
            else if (extension == ".pptx")
            {
                return GetPowerPointSlideCount(file);
            }
            else if (extension == ".xlsx")
            {
                return GetExcelSheetCount(file);
            }
            else if (extension == ".pdf")
            {
                return GetPdfPageCount(file);
            }

            return 0;
        }

        private static int GetWordPageCount(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                using (var wordDocument = WordprocessingDocument.Open(stream, false))
                {
                    var mainPart = wordDocument.ExtendedFilePropertiesPart;
                    if (mainPart != null)
                    {
                        var properties = mainPart.Properties;
                        var pagesElement = properties?.Pages;

                        if (pagesElement != null && int.TryParse(pagesElement.Text, out int pageCount))
                        {
                            return pageCount;
                        }
                    }
                }
            }
            return 0;
        }


        private static int GetPowerPointSlideCount(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                using (var presentationDocument = PresentationDocument.Open(stream, false))
                {
                    return presentationDocument.PresentationPart?.SlideParts?.Count() ?? 0;
                }
            }
        }

        private static int GetExcelSheetCount(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                using (var spreadsheetDocument = SpreadsheetDocument.Open(stream, false))
                {
                    var sheets = spreadsheetDocument.WorkbookPart?.Workbook.Sheets;
                    return sheets?.Count() ?? 0;
                }
            }
        }

        private static int GetPdfPageCount(IFormFile file)
        {
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var reader = new iText.Kernel.Pdf.PdfReader(stream))
                    {
                        var pdfDocument = new iText.Kernel.Pdf.PdfDocument(reader);
                        return pdfDocument.GetNumberOfPages();
                    }
                }
            }
            catch
            {
                return 0;
            }
        }



    }

    public static class FileReader
    {
        public static string ReadFileContent(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "File is empty or null.";
            }

            string extension = Path.GetExtension(file.FileName).ToLower();
            using var stream = file.OpenReadStream();
            return extension switch
            {
                ".docx" => ReadDocx(stream),
                ".pptx" => ReadPptx(stream),
                ".xlsx" => ReadExcel(stream),
                _ => "Unsupported file format."
            };
        }
        private static string ReadDocx(Stream stream)
        {
            var sb = new StringBuilder();

            using (var document = WordprocessingDocument.Open(stream, false))
            {
                var body = document.MainDocumentPart?.Document.Body;
                if (body != null)
                {
                    foreach (var paragraph in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                    {
                        var paragraphText = string.Join(" ", paragraph.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>()
                                                                    .Select(t => t.Text.Trim()));
                        if (!string.IsNullOrWhiteSpace(paragraphText))
                        {
                            sb.AppendLine(paragraphText);
                        }
                    }
                }
            }

            return CleanString(sb.ToString());
        }
        private static string ReadPptx(Stream stream)
        {
            var sb = new StringBuilder();

            using (var presentation = PresentationDocument.Open(stream, false))
            {
                var slideIdList = presentation.PresentationPart.Presentation.SlideIdList;

                if (slideIdList != null)
                {
                    foreach (var slideId in slideIdList.Elements<SlideId>())
                    {
                        var slidePart = (SlidePart)presentation.PresentationPart.GetPartById(slideId.RelationshipId);

                        if (slidePart != null)
                        {
                            foreach (var paragraph in slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                            {
                                var paragraphText = string.Join(" ", paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Text>()
                                                                            .Select(t => t.Text.Trim()));

                                if (!string.IsNullOrWhiteSpace(paragraphText))
                                {
                                    sb.Append(paragraphText).Append(" ");
                                }
                            }

                            sb.AppendLine();
                        }
                    }
                }
            }

            return CleanString(sb.ToString());
        }

        private static string ReadExcel(Stream stream)
        {
            var sb = new StringBuilder();
            using (var document = SpreadsheetDocument.Open(stream, false))
            {
                var workbookPart = document.WorkbookPart;
                if (workbookPart != null)
                {
                    foreach (var sheet in workbookPart.Workbook.Sheets.OfType<DocumentFormat.OpenXml.Spreadsheet.Sheet>())
                    {
                        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                        var rows = worksheetPart.Worksheet.Descendants<Row>();

                        foreach (var row in rows)
                        {
                            foreach (var cell in row.Descendants<Cell>())
                            {
                                var cellValue = ReadCell(cell, workbookPart);
                                if (!string.IsNullOrWhiteSpace(cellValue))
                                {
                                    sb.Append(cellValue.Trim()).Append(" ");
                                }
                            }
                        }
                    }
                }
            }
            return CleanString(sb.ToString());
        }

        private static string ReadCell(Cell cell, WorkbookPart workbookPart)
        {
            if (cell.CellValue == null) return string.Empty;

            var value = cell.CellValue.Text;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
                if (sharedStringTable != null)
                {
                    return sharedStringTable.ElementAt(int.Parse(value)).InnerText;
                }
            }

            return value;
        }

        private static string CleanString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            return string.Join(" ", input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                        .ToLower();
        }
    }
}