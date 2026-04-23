using Libreria.Data;
using Libreria.Models;
using Libreria.Response;
using Microsoft.EntityFrameworkCore;

namespace Libreria.Service;

public class UserService(MysqlDbContext context)
{
    public ServiceResponse<IEnumerable<User>> GetAllUsers()
    {
        var users = context.users
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Id)
            .ToList();

        return new ServiceResponse<IEnumerable<User>>()
        {
            Success = true,
            Data = users,
        };
    }

    public ServiceResponse<User> GetById(int id)
    {
        var response = new ServiceResponse<User>();
        var user = context.users
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == id && !x.IsDeleted);

        if (user is null)
        {
            response.Success = false;
            response.Message = "Usuario no encontrado.";
            return response;
        }

        response.Success = true;
        response.Data = user;
        response.Message = string.Empty;
        return response;
    }

    public ServiceResponse<User> SaveUser(User user)
    {
        var response = new ServiceResponse<User>();
        user.Name = user.Name.Trim();
        user.Email = user.Email.Trim();
        user.Password = user.Password.Trim();

        if (string.IsNullOrWhiteSpace(user.Name) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(user.Password))
        {
            response.Success = false;
            response.Message = "Todos los campos del usuario son obligatorios.";
            return response;
        }

        var normalizedEmail = user.Email.ToLower();
        var activeEmailExists = context.users.AsNoTracking().Any(x => !x.IsDeleted && x.Email.ToLower() == normalizedEmail);
        if (activeEmailExists)
        {
            response.Success = false;
            response.Message = "Ya existe un usuario con ese correo.";
            return response;
        }

        var deletedEmailExists = context.users.AsNoTracking().Any(x => x.IsDeleted && x.Email.ToLower() == normalizedEmail);
        if (deletedEmailExists)
        {
            response.Success = false;
            response.Message = "correo baneado del sistema";
            return response;
        }

        user.IsDeleted = false;
        context.users.Add(user);
        context.SaveChanges();
        response.Success = true;
        response.Data = user;
        response.Message = "Usuario creado correctamente.";
        return response;
    }

    public ServiceResponse<User> UpgradeUser(User user)
    {
        var response = new ServiceResponse<User>();
        var currentUser = context.users.FirstOrDefault(x => x.Id == user.Id && !x.IsDeleted);
        if (currentUser is null)
        {
            response.Success = false;
            response.Message = "Usuario no encontrado.";
            return response;
        }

        user.Name = user.Name.Trim();
        user.Email = user.Email.Trim();
        user.Password = user.Password.Trim();

        if (string.IsNullOrWhiteSpace(user.Name) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(user.Password))
        {
            response.Success = false;
            response.Message = "Todos los campos del usuario son obligatorios.";
            return response;
        }

        var normalizedEmail = user.Email.ToLower();
        var emailUsed = context.users.AsNoTracking().Any(x =>
            !x.IsDeleted &&
            x.Id != user.Id &&
            x.Email.ToLower() == normalizedEmail);

        if (emailUsed)
        {
            response.Success = false;
            response.Message = "El correo ya está asignado a otro usuario.";
            return response;
        }

        var deletedEmailExists = context.users.AsNoTracking().Any(x =>
            x.IsDeleted &&
            x.Id != user.Id &&
            x.Email.ToLower() == normalizedEmail);

        if (deletedEmailExists)
        {
            response.Success = false;
            response.Message = "correo baneado del sistema";
            return response;
        }

        currentUser.Name = user.Name;
        currentUser.Email = user.Email;
        currentUser.Password = user.Password;

        context.SaveChanges();
        response.Success = true;
        response.Data = currentUser;
        response.Message = "Usuario actualizado correctamente.";
        return response;
    }

    public ServiceResponse<bool> Delete(int id)
    {
        var response = new ServiceResponse<bool>();
        var user = context.users.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        if (user is null)
        {
            response.Success = false;
            response.Message = "Usuario no encontrado.";
            return response;
        }

        var activeLoans = context.records
            .Where(x => !x.IsDeleted && x.IsActive && x.UserId == id)
            .ToList();

        foreach (var loan in activeLoans)
        {
            loan.IsActive = false;
        }

        user.IsDeleted = true;
        context.SaveChanges();
        response.Success = true;
        response.Data = true;
        response.Message = "Usuario eliminado correctamente.";
        return response;
    }
}
