namespace GGVolt.Core.Constants;

public static class CoreConstants
{
    public static class Api
    {
        public const string Base = "api/v1";
        public const string Auth = $"{Base}/auth";
        public const string Games = $"{Base}/games";
        public const string Library = $"{Base}/library";
        public const string Payments = $"{Base}/payments";
        public const string Downloads = $"{Base}/downloads";
    }

    public static class Limits
    {
        public const int MinUsernameLength = 3;
        public const int MaxUsernameLength = 32;
        public const int MinPasswordLength = 6;
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 20;
    }

    public static class Storage
    {
        public const string DefaultContentType = "application/octet-stream";
        public const long MaxUploadSizeBytes = 10_737_418_240; // 10 GB
    }
}