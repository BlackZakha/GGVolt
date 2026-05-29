namespace GGVolt.Infrastructure.Storage;

public class MinioSettings
{
    public const string Section = "Minio";
    
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin123";
    public string BucketName { get; set; } = "ggvolt-content";
    public bool UseSsl { get; set; } = false;
    public string Region { get; set; } = "us-east-1";
    public int PresignedUrlExpiryMinutes { get; set; } = 15;
}