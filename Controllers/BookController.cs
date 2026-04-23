using Libreria.Models;
using Libreria.Service;
using Microsoft.AspNetCore.Mvc;

namespace Libreria.Controllers;

public class BookController(BookService bookService) : Controller
{
    public IActionResult Index()
    {
        var response = bookService.GetAllBooks();
        return View(response.Data ?? []);
    }

    public IActionResult Create()
    {
        return View(new Book());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Book book)
    {
        if (!ModelState.IsValid)
        {
            return View(book);
        }

        var response = bookService.SaveBook(book);
        if (!response.Success)
        {
            ModelState.AddModelError(string.Empty, response.Message);
            return View(book);
        }

        TempData["Message"] = response.Message;
        TempData["Success"] = response.Success;
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Show(int id)
    {
        var response = bookService.GetById(id);
        if (!response.Success || response.Data is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(response.Data);
    }

    public IActionResult Edit(int id)
    {
        var response = bookService.GetById(id);
        if (!response.Success || response.Data is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Book book)
    {
        if (id != book.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(book);
        }

        var response = bookService.UpgradeBook(book);
        if (!response.Success)
        {
            ModelState.AddModelError(string.Empty, response.Message);
            return View(book);
        }

        TempData["Message"] = response.Message;
        TempData["Success"] = response.Success;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Destroy(int id)
    {
        var response = bookService.Delete(id);
        TempData["Message"] = response.Message;
        TempData["Success"] = response.Success;
        return RedirectToAction(nameof(Index));
    }
}
