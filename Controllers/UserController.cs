using Libreria.Models;
using Libreria.Service;
using Microsoft.AspNetCore.Mvc;

namespace Libreria.Controllers;

public class UserController(UserService userService) : Controller
{
    public IActionResult Index()
    {
        var response = userService.GetAllUsers();
        return View(response.Data ?? []);
    }

    public IActionResult Create()
    {
        return View(new User());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(User user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        var response = userService.SaveUser(user);
        if (!response.Success)
        {
            ModelState.AddModelError(string.Empty, response.Message);
            return View(user);
        }

        TempData["Message"] = response.Message;
        TempData["Success"] = response.Success;
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Show(int id)
    {
        var response = userService.GetById(id);
        if (!response.Success || response.Data is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(response.Data);
    }

    public IActionResult Edit(int id)
    {
        var response = userService.GetById(id);
        if (!response.Success || response.Data is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, User user)
    {
        if (id != user.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(user);
        }

        var response = userService.UpgradeUser(user);
        if (!response.Success)
        {
            ModelState.AddModelError(string.Empty, response.Message);
            return View(user);
        }

        TempData["Message"] = response.Message;
        TempData["Success"] = response.Success;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Destroy(int id)
    {
        var response = userService.Delete(id);
        TempData["Message"] = response.Message;
        TempData["Success"] = response.Success;
        return RedirectToAction(nameof(Index));
    }
}
