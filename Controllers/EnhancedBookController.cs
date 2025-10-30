using BookVectorMVC.Models;
using BookVectorMVC.Services;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BookVectorMVC.Controllers
{
    public class EnhancedBookController : Controller
    {
        private EnhancedBookService enhancedBookService = new EnhancedBookService();
        private BookService bookService = new BookService();

        public ActionResult Enhanced()
        {
            var books = LoadBooks();
            ViewBag.Statistics = enhancedBookService.GetSystemStatistics();
            return View("~/Views/Book/Enhanced.cshtml", books);
        }

        private List<Book> LoadBooks()
        {
            try
            {
                return bookService.GetAllBooks();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "載入書籍發生異常: " + ex.Message;
                return new List<Book>();
            }
        }

        [HttpPost]
        public ActionResult Add(string title, string description, string position)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "書名不能為空";
                return RedirectToAction("Enhanced");
            }

            try
            {
                bookService.AddBook(title, description, position);
                TempData["Success"] = $"已新增《{title}》";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "新增時發生問題: " + ex.Message;
            }

            return RedirectToAction("Enhanced");
        }

        [HttpPost]
        public ActionResult Search(string query, string position, string title)
        {
            List<SearchResult> results = new List<SearchResult>();

            try
            {
                if (!string.IsNullOrWhiteSpace(position))
                {
                    results = bookService.SearchByPosition(position);
                    ViewBag.SearchMethod = "依位置搜尋";
                }
                else if (!string.IsNullOrWhiteSpace(title))
                {
                    results = bookService.SearchByTitle(title);
                    ViewBag.SearchMethod = "依書名搜尋";
                }
                else if (!string.IsNullOrWhiteSpace(query))
                {
                    results = bookService.SearchByVector(query, 10);
                    ViewBag.SearchMethod = "依向量搜尋";
                    ViewBag.SearchQuery = query;
                }
                else
                {
                    ViewBag.SearchMethod = "請輸入搜尋條件";
                }

                ViewBag.ResultCount = results.Count;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "搜尋發生異常: " + ex.Message;
            }

            return PartialView("~/Views/Book/_EnhancedSearchResult.cshtml", results);
        }
    }
}