using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GGVolt.Client.Models.Api;

public record PagedResponse<T>(
    [property: JsonPropertyName("items")] List<T> Items,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize
);