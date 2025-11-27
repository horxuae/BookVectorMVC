using BookVectorMVC.Controllers;

namespace BookVectorMVC.Services.Interfaces
{
    public interface IBookSearchService
    {
        Task<List<BookSearchResult>> SearchOnlineBooks(string query);
    }
}