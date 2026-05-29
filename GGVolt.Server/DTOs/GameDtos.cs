namespace GGVolt.Server.DTOs;

public record GameDto(
    Guid Id, 
    string Title, 
    string Description, 
    decimal Price, 
    string CoverUrl, 
    DateTime ReleaseDate, 
    GGVolt.Core.Enums.ContentType Type);

public record PagedResponse<T>(List<T> Items, int TotalCount, int Page, int PageSize);
public record DownloadLinkResponse(string Url, TimeSpan ExpiresIn, long SizeBytes);