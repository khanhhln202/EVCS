namespace EVCS.Web.ViewModels;


public class PagedResult
{
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required string BasePath { get; init; }
    public required IQueryCollection Query { get; init; }
    public string UrlForPage(int page)
    {
        var qs = Query.ToDictionary(k => k.Key, v => v.Value.ToString());
        qs["page"] = page.ToString();
        var query = string.Join('&', qs.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        return $"{BasePath}?{query}";
    }
}