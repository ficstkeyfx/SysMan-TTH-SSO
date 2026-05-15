using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace SysMan.Libs
{
    public class XuatDonNghiPhep
    {


        public void GenerateReport(string templatePath, string outputPath, Dictionary<string, string> mergeFields)
        {
            // Copy khung từ file mẫu, từ đó tạo file mới
            File.Copy(templatePath, outputPath, true);

            // Mở file vừa tạo để chỉnh sửa
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(outputPath, true))
            {
                // Truy cập vào file mới
                var mainPart = wordDoc.MainDocumentPart;
                var document = mainPart.Document;

                // duyệt qua từng field để điền thông tin từ cơ sở dữ liệu
                foreach (var field in mergeFields)
                {
                    ReplacePlaceholder(document, field.Key, field.Value);
                }

                // Lưu file word
                document.Save();
            }
        }


        private void ReplacePlaceholder(Document document, string placeholder, string value)
        {
            foreach (var text in document.Descendants<Text>())
            {
                if (text.Text.Contains(placeholder))
                {
                    text.Text = text.Text.Replace(placeholder, value);
                }
            }
        }

        private void OpenDocument(string filePath) 
        { 
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        } 
        
    }
}
