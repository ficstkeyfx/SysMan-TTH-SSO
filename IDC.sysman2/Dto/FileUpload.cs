namespace SysMan.Dto;
public class FileUpload
{
    public string uid { get; set; } = " ";
    public string fileName { get; set; } = " ";
    public string bucketName { get; set; } = " ";
    public string hash { get; set; }
    public int? pageCount { get; set; }
    public string? loaiFile { get; set; }
    public string? fileContent { get; set; }
    // public long? fileSize { get; set; }

}