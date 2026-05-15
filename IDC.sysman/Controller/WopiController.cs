// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Minio;
// using Minio.DataModel.Args;
// using SysMan.Models;
// using SysMan.Services.LinkService;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Serialization;

// namespace SysMan
// {
//     [Route("wopi/files")]
//     public class WopiController : Controller
//     {
//         private readonly IMinioClient _minioClient;


//         public WopiController()
//         {
//             IConfigurationRoot configuration = new ConfigurationBuilder()
//                 .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
//                 .AddJsonFile("appsettings.json")
//                 .Build();

//             _minioClient = new MinioClient()
//                 .WithEndpoint(configuration.GetConnectionString("MinioEndpoint"))
//                 .WithCredentials(configuration.GetConnectionString("MinioAccessKey"), configuration.GetConnectionString("MinioSecretKey"))
//                 .WithSSL(false).Build();
//         }

        
//         [HttpGet("{folderName}/{fileId}")]
//         public async Task<IActionResult> GetFileInfo(string folderName, string fileId, [FromQuery] string access_token)
//         {
//             if (string.IsNullOrEmpty(access_token) || access_token != "your-secure-token")
//             {
//                 return Unauthorized();
//             }

//             try
//             {
//                 string minioPath = $"{folderName}/{fileId}";

//                 var stat = await _minioClient.StatObjectAsync(new StatObjectArgs()
//                     .WithBucket("2025")
//                     .WithObject(minioPath));

//                 string mimeType = GetMimeType(fileId);
                
//                 var fileInfo = new
//                 {
//                     BaseFileName = Path.GetFileName(fileId),
//                     Size = stat.Size,
//                     OwnerId = "user123",
//                     Version = stat.LastModified.ToString("o"),
//                     UserCanWrite = true,
//                     UserFriendlyName = "The Rock",
//                     MimeType = mimeType
//                 };

//                 return Content(JsonConvert.SerializeObject(fileInfo), "application/json");
//             }
//             catch (Exception ex)
//             {
//                 return NotFound($"File not found: {ex.Message}");
//             }
//         }

//         [HttpGet("{folderName}/{fileId}/contents")]
//         public async Task<IActionResult> GetFileContent(string folderName, string fileId, [FromQuery] string access_token)
//         {
//             // if (string.IsNullOrEmpty(access_token)  || access_token != "your-secure-token")
//             // {
//             //     return Unauthorized("Invalid access token");
//             // }

//             try
//             {
//                 string minioPath = $"{folderName}/{fileId}";
//                 Stream fileStream = new MemoryStream();

//                 await _minioClient.GetObjectAsync(new GetObjectArgs()
//                     .WithBucket("2025")
//                     .WithObject(minioPath)
//                     .WithCallbackStream(stream => stream.CopyTo(fileStream)));

//                 fileStream.Position = 0; // Đảm bảo stream bắt đầu từ đầu

//                 string mimeType = GetMimeType(fileId);
//                 return File(fileStream, mimeType, fileId);
//             }
//             catch (Exception ex)
//             {
//                 return NotFound($"Error retrieving file: {ex.Message}");
//             }
//         }


//         [HttpPost("{folderName}/{fileId}/contents")]
//         public async Task<IActionResult> PutFile(string folderName, string fileId, [FromQuery] string access_token)
//         {
//             // if (string.IsNullOrEmpty(access_token)  || access_token != "your-secure-token")
//             // {
//             //     return Unauthorized("Invalid access token");
//             // }

//             try
//             {
//                 string minioPath = $"{folderName}/{fileId}";
//                 string mimeType = GetMimeType(fileId);

//                 using var stream = Request.Body;
//                 await _minioClient.PutObjectAsync(new PutObjectArgs()
//                     .WithBucket("2025")
//                     .WithObject(minioPath)
//                     .WithStreamData(stream)
//                     .WithContentType(mimeType)
//                     .WithObjectSize(Request.ContentLength ?? -1));

//                 return Ok(new { LastModifiedTime = DateTime.UtcNow.ToString("o") });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Error saving file: {ex.Message}");
//             }
//         }



//         private static string GetMimeType(string fileName)
//         {
//             var extension = Path.GetExtension(fileName).ToLower();
//             return extension switch
//             {
//                 ".doc" => "application/msword",
//                 ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
//                 ".xls" => "application/vnd.ms-excel",
//                 ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                 ".ppt" => "application/vnd.ms-powerpoint",
//                 ".pdf" => "application/pdf",
//                 ".txt" => "text/plain",
//                 _ => "application/octet-stream"
//             };
//         }
//     }
// }