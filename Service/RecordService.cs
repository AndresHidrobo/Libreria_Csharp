using Libreria.Data;
using Libreria.Models;
using Libreria.Response;
using Microsoft.EntityFrameworkCore;

namespace Libreria.Service;

public class RecordService(MysqlDbContext context)
{
    public ServiceResponse<IEnumerable<Record>> GetAllRecords()
    {
        var records = context.records
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Book)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .ToList();

        return new ServiceResponse<IEnumerable<Record>>()
        {
            Success = true,
            Data = records,
        };
    }

    public ServiceResponse<Record> GetById(int id)
    {
        var response = new ServiceResponse<Record>();
        var record = context.records
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Book)
            .FirstOrDefault(x => x.Id == id && !x.IsDeleted);

        if (record is null)
        {
            response.Success = false;
            response.Message = "Préstamo no encontrado.";
            return response;
        }

        response.Success = true;
        response.Data = record;
        response.Message = string.Empty;
        return response;
    }

    public ServiceResponse<Record> SaveRecord(Record record)
    {
        var response = new ServiceResponse<Record>();
        var userExists = context.users.AsNoTracking().Any(x => x.Id == record.UserId && !x.IsDeleted);
        var bookExists = context.books.AsNoTracking().Any(x => x.Id == record.BookId && !x.IsDeleted);

        if (!userExists || !bookExists)
        {
            response.Success = false;
            response.Message = "Debe seleccionar un usuario y un libro válidos.";
            return response;
        }

        if (HasActiveConflict(record.UserId, record.BookId))
        {
            response.Success = false;
            response.Message = "No se puede crear el préstamo porque el usuario o el libro ya tienen un préstamo activo.";
            return response;
        }

        record.CreatedAt = DateTime.Now;
        record.IsActive = true;
        record.IsDeleted = false;
        context.records.Add(record);
        context.SaveChanges();
        response.Success = true;
        response.Data = record;
        response.Message = "Préstamo creado correctamente.";
        return response;
    }

    public ServiceResponse<Record> UpgradeRecord(Record record)
    {
        var response = new ServiceResponse<Record>();
        var currentRecord = context.records.FirstOrDefault(x => x.Id == record.Id && !x.IsDeleted);
        if (currentRecord is null)
        {
            response.Success = false;
            response.Message = "Préstamo no encontrado.";
            return response;
        }

        var userExists = context.users.AsNoTracking().Any(x => x.Id == record.UserId && !x.IsDeleted);
        var bookExists = context.books.AsNoTracking().Any(x => x.Id == record.BookId && !x.IsDeleted);

        if (!userExists || !bookExists)
        {
            response.Success = false;
            response.Message = "Debe seleccionar un usuario y un libro válidos.";
            return response;
        }

        if (record.IsActive && HasActiveConflict(record.UserId, record.BookId, record.Id))
        {
            response.Success = false;
            response.Message = "No se puede actualizar el préstamo porque el usuario o el libro ya tienen otro préstamo activo.";
            return response;
        }

        currentRecord.UserId = record.UserId;
        currentRecord.BookId = record.BookId;
        context.SaveChanges();
        response.Success = true;
        response.Data = currentRecord;
        response.Message = "Préstamo actualizado correctamente.";
        return response;
    }

    public ServiceResponse<bool> Delete(int id)
    {
        var response = new ServiceResponse<bool>();
        var record = context.records.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        if (record is null)
        {
            response.Success = false;
            response.Message = "Préstamo no encontrado.";
            return response;
        }

        record.IsDeleted = true;
        context.SaveChanges();
        response.Success = true;
        response.Data = true;
        response.Message = "Préstamo eliminado correctamente.";
        return response;
    }

    public ServiceResponse<Record> FinishLoan(int id)
    {
        var response = new ServiceResponse<Record>();
        var record = context.records.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        if (record is null)
        {
            response.Success = false;
            response.Message = "Préstamo no encontrado.";
            return response;
        }

        if (!record.IsActive)
        {
            response.Success = false;
            response.Message = "El préstamo ya está finalizado.";
            return response;
        }

        record.IsActive = false;
        context.SaveChanges();
        response.Success = true;
        response.Data = record;
        response.Message = "Préstamo finalizado correctamente.";
        return response;
    }

    private bool HasActiveConflict(int userId, int bookId, int currentRecordId = 0)
    {
        return context.records.AsNoTracking().Any(x =>
            !x.IsDeleted &&
            x.IsActive &&
            x.Id != currentRecordId &&
            (x.UserId == userId || x.BookId == bookId));
    }
}
