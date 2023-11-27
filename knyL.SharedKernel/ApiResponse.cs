using Microsoft.AspNetCore.WebUtilities;

namespace knyL.SharedKernel;

public interface IApiResponse
{
    bool Success { get; set; }
    string Code { get; set; }
    string? Message { get; set; }
    ICollection<string?>? Messages { get; set; }
    static abstract IApiResponse MarkAsErrors(string? message);
}
public interface IApiResponse<T> : IApiResponse
{
    T? Data { get; set; }
}

public interface IPagedApiResponse<T> : IApiResponse<ICollection<T?>>
{
    Pagination? Pagination { get; set; }

}
public record ApiResponse : IApiResponse
{
    public bool Success { get; set; }
    public string Code { get; set; } = "SUCCESS";
    public string? Message { get; set; }
    public ICollection<string?>? Messages { get; set; }
    public static IApiResponse MarkAsErrors(string? message)
    {
        return new ApiResponse
        {
            Success = false,
            Code = "ERROR",
            Message = "Oops! Something went wrong.",
            Messages = new List<string?>() { message }
        };
    }
}
public record ApiResponse<T> : ApiResponse, IApiResponse<T>
{
    public T? Data { get; set; }
    public ApiResponse(T? data)
    {
        Success = true;
        Data = data;
    }
}

public record PagedApiResponse<T> : ApiResponse, IPagedApiResponse<T?>
{
    public ICollection<T?>? Data { get; set; }
    public Pagination? Pagination { get; set; }
    public PagedApiResponse(ICollection<T?>? data, Uri? baseUri, int? totalItems, int? page, int? pageSize)
    {
        Success = true;
        Data = data;
        if (data is { Count: > 0 })
        {
            Pagination = new Pagination(baseUri, totalItems ?? data.Count, page ?? 1, pageSize ?? 20);
        }
    }
}

public record Pagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public string? NextPage { get; set; }
    public string? PreviousPage { get; set; }
    public string? FirstPage { get; set; }
    public string? LastPage { get; set; }
    public Pagination(Uri? baseUri, int totalItems, int page = 1, int pageSize = 20)
    {
        Page = page;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        if (baseUri is { })
        {
            var createPageUri = (Uri baseUri, int page, int pageSize) =>
            {
                var modifiedUri = QueryHelpers.AddQueryString(baseUri.ToString(), "page", page.ToString());
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", pageSize.ToString());
                return new Uri(modifiedUri);
            };

            NextPage = page < TotalPages ? createPageUri(baseUri, page + 1, pageSize).ToString() : null;
            PreviousPage = page > 1 ? createPageUri(baseUri, page - 1, pageSize).ToString() : null;
            FirstPage = createPageUri(baseUri, 1, pageSize).ToString();
            LastPage = createPageUri(baseUri, TotalPages, pageSize).ToString();
        }
    }
}