using Libreria.Data;
using Libreria.Models;
using Libreria.Response;
using Microsoft.EntityFrameworkCore;

namespace Libreria.Service;

public class BookService(MysqlDbContext context)
{
    public ServiceResponse<IEnumerable<Book>> GetAllBooks()
    {
        var books = context.books
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Id)
            .ToList();

        return new ServiceResponse<IEnumerable<Book>>()
        {
            Success = true,
            Data = books,
        };
    }

    public ServiceResponse<Book> GetById(int id)
    {
        var response = new ServiceResponse<Book>();
        var book = context.books
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == id && !x.IsDeleted);

        if (book is null)
        {
            response.Success = false;
            response.Message = "Libro no encontrado.";
            return response;
        }

        response.Success = true;
        response.Data = book;
        response.Message = string.Empty;
        return response;
    }

    public ServiceResponse<Book> SaveBook(Book book)
    {
        var response = new ServiceResponse<Book>();
        NormalizeBook(book);
        var duplicateBook = ExistsDuplicateBook(book);
        if (duplicateBook)
        {
            response.Success = false;
            response.Message = "Ya existe un libro con la combinación título, autor y editorial.";
            return response;
        }

        var deletedBookExists = ExistsDeletedBook(book);
        if (deletedBookExists)
        {
            response.Success = false;
            response.Message = "Libro sin existencias";
            return response;
        }

        book.IsDeleted = false;
        context.books.Add(book);
        context.SaveChanges();
        response.Success = true;
        response.Data = book;
        response.Message = "Libro creado correctamente.";
        return response;
    }

    public ServiceResponse<Book> UpgradeBook(Book book)
    {
        var response = new ServiceResponse<Book>();
        var currentBook = context.books.FirstOrDefault(x => x.Id == book.Id && !x.IsDeleted);
        if (currentBook is null)
        {
            response.Success = false;
            response.Message = "Libro no encontrado.";
            return response;
        }

        NormalizeBook(book);
        var duplicateBook = ExistsDuplicateBook(book, book.Id);
        if (duplicateBook)
        {
            response.Success = false;
            response.Message = "Ya existe un libro con la combinación título, autor y editorial.";
            return response;
        }

        var deletedBookExists = ExistsDeletedBook(book, book.Id);
        if (deletedBookExists)
        {
            response.Success = false;
            response.Message = "Libro sin existencias";
            return response;
        }

        currentBook.Title = book.Title;
        currentBook.Description = book.Description;
        currentBook.Author = book.Author;
        currentBook.Editorial = book.Editorial;

        context.SaveChanges();
        response.Success = true;
        response.Data = currentBook;
        response.Message = "Libro actualizado correctamente.";
        return response;
    }

    public ServiceResponse<bool> Delete(int id)
    {
        var response = new ServiceResponse<bool>();
        var book = context.books.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        if (book is null)
        {
            response.Success = false;
            response.Message = "Libro no encontrado.";
            return response;
        }

        var activeLoans = context.records
            .Where(x => !x.IsDeleted && x.IsActive && x.BookId == id)
            .ToList();

        foreach (var loan in activeLoans)
        {
            loan.IsActive = false;
        }

        book.IsDeleted = true;
        context.SaveChanges();
        response.Success = true;
        response.Data = true;
        response.Message = "Libro eliminado correctamente.";
        return response;
    }

    private void NormalizeBook(Book book)
    {
        book.Title = book.Title.Trim();
        book.Description = book.Description.Trim();
        book.Author = book.Author.Trim();
        book.Editorial = book.Editorial.Trim();
    }

    private bool ExistsDuplicateBook(Book book, int currentBookId = 0)
    {
        if (string.IsNullOrWhiteSpace(book.Title) ||
            string.IsNullOrWhiteSpace(book.Author) ||
            string.IsNullOrWhiteSpace(book.Editorial))
        {
            return true;
        }

        var normalizedTitle = book.Title.ToLower();
        var normalizedAuthor = book.Author.ToLower();
        var normalizedEditorial = book.Editorial.ToLower();

        return context.books.AsNoTracking().Any(x =>
            !x.IsDeleted &&
            x.Id != currentBookId &&
            x.Title.ToLower() == normalizedTitle &&
            x.Author.ToLower() == normalizedAuthor &&
            x.Editorial.ToLower() == normalizedEditorial);
    }

    private bool ExistsDeletedBook(Book book, int currentBookId = 0)
    {
        if (string.IsNullOrWhiteSpace(book.Title) ||
            string.IsNullOrWhiteSpace(book.Author) ||
            string.IsNullOrWhiteSpace(book.Editorial))
        {
            return false;
        }

        var normalizedTitle = book.Title.ToLower();
        var normalizedAuthor = book.Author.ToLower();
        var normalizedEditorial = book.Editorial.ToLower();

        return context.books.AsNoTracking().Any(x =>
            x.IsDeleted &&
            x.Id != currentBookId &&
            x.Title.ToLower() == normalizedTitle &&
            x.Author.ToLower() == normalizedAuthor &&
            x.Editorial.ToLower() == normalizedEditorial);
    }
}
