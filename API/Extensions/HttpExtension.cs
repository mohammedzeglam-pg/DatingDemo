using System.Text.Json;
using API.Helpers;
using Microsoft.AspNetCore.Http;

namespace API.Extensions
{
  public static class HttpExtenstion
  {
    public static void AddPaginationHeader(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages)
    {
      PaginationHeader paginationHeader = new(currentPage, itemsPerPage, totalItems, totalPages);
      JsonSerializerOptions options = new()
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };
      response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader, options));
      response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
    }
  }
}
