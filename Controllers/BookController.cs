using BookVectorMVC.Models;
using BookVectorMVC.Services;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BookVectorMVC.Controllers
{
    public class BookController : Controller
    {
        private BookService bookService = new BookService();

        public ActionResult Index()
        {
            var books = bookService.GetAllBooks();
            return View(books);
        }

        [HttpPost]
        public ActionResult Add(string title, string description, string position)
        {
            bookService.AddBook(title, description, position);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Search(string query, string position, string title)
        {
            List<SearchResult> results = null;

            if (!string.IsNullOrWhiteSpace(position))
            {
                results = bookService.SearchByPosition(position);
            }
            else if (!string.IsNullOrWhiteSpace(title))
            {
                results = bookService.SearchByTitle(title);
            }
            else
            {
                results = bookService.SearchByVector(query);
            }

            return PartialView("_SearchResult", results);
        }
    }
}
