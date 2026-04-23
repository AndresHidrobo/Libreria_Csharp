using Libreria.Models;
using Libreria.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Libreria.Controllers;

public class RecordController(
    RecordService recordService,
    UserService userService,
    BookService bookService) : Controller
{
    public IActionResult Index()
    {
        var response = recordService.GetAllRecords();
        return View(response.Data ?? []);
    }

    public IActionResult Create()
    {
        LoadRelations();
        return View(new Record { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Record record)
    {
        record.IsActive = true;

        if (!ModelState.IsValid)
        {
            LoadRelations();
            return View(record);
        }

        var response = recordService.SaveRecord(record);
        if (!response.Success)
        {
            ModelState.AddModelError(string.Empty, response.Message);
            LoadRelations();
            return View(record);
        }

        TempData["Message"] = response.Message;
        TempData["Success"] = response.Success;
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Show(int id)
    {
        var response = recordService.GetById(id);
        if (!response.Success || response.Data is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(response.Data);
    }

    public IActionResult Edit(int id)
    {
        var response = recordService.GetById(id);
        if (!response.Success || response.Data is null)
        {
            return RedirectToAction(nameof(Index));
        }

        LoadRelations();
        return View(response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Record record)
    {
        if (id != record.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            LoadRelations();
            return View(record);
        }

        var response = recordService.UpgradeRecord(record);
        if (!response.Success)
        {
            ModelState.AddModelError(string.Empty, response.Message);
            LoadRelations();
            return View(record);
        }

        TempData["Message"] = response.Message;
        TempData["Success"] = response.Success;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Destroy(int id)
    {
        var response = recordService.Delete(id);
        TempData["Message"] = response.Message;
        TempData["Success"] = response.Success;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FinishLoan(int id)
    {
        var response = recordService.FinishLoan(id);
        TempData["Message"] = response.Message;
        TempData["Success"] = response.Success;
        return RedirectToAction(nameof(Index));
    }

    private void LoadRelations()
    {
        var users = userService.GetAllUsers();
        var books = bookService.GetAllBooks();

        var userOptions = (users.Data ?? [])
            .Select(user => new
            {
                Id = user.Id,
                Description = $"{user.Id} - {user.Name} - {user.Email}"
            })
            .ToList();

        var bookOptions = (books.Data ?? [])
            .Select(book => new
            {
                Id = book.Id,
                Description = $"{book.Title} - {book.Author} - {book.Editorial}"
            })
            .ToList();

        ViewBag.Users = new SelectList(userOptions, "Id", "Description");
        ViewBag.Books = new SelectList(bookOptions, "Id", "Description");
    }
}
