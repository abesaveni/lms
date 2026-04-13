namespace LiveExpert.Application.Common;

public class AffindaApiException : Exception
{
    public System.Net.HttpStatusCode StatusCode { get; }
    public string? Code { get; }
    public string? Detail { get; }
    public string? RawResponse { get; }

    public AffindaApiException(System.Net.HttpStatusCode statusCode, string? code, string? detail, string? rawResponse)
        : base(detail ?? rawResponse ?? "Affinda API error")
    {
        StatusCode = statusCode;
        Code = code;
        Detail = detail;
        RawResponse = rawResponse;
    }
}
