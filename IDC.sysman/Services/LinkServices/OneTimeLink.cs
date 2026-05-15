public class OneTimeLink
{
    public string Token { get; set; }
    public string ObjectName { get; set; }
    public string BucketName {get; set;}
    public DateTime ExpirationDate { get; set; }
    public bool IsUsed { get; set; }
}

public class LinkObject 
{
    public string ObjectName { get; set; }
    public string BucketName {get; set;}
}
