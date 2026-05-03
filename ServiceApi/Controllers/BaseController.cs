using System.Linq.Expressions;
using DataLayer;
using DataLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ServiceApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class BaseController : Controller
{
    protected string CurrentUserEmail
    {
        get
        {
            var jwtEmail = User.Claims.FirstOrDefault(c =>
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            return jwtEmail?.Value;
        }
    }

    protected User CurrentUser
    {
        get
        {
            using var context = new NotebookContext();
            return context.Users.First(u => u.Email == CurrentUserEmail);
        }
    }

    protected ActionResult<T> Get<T>(DbSet<T> set, int id) where T : BaseEntity
    {
        var obj = set.Find(id);

        if (obj == null) return NotFound();

        return obj;
    }

    protected ActionResult<T> Create<T>(DbContext context, T obj, string routePattern) where T : BaseEntity
    {
        context.Add(obj);
        context.SaveChanges();

        return Created(string.Format(routePattern, obj.Id), obj);
    }

    protected ActionResult Update<T>(DbContext context, DbSet<T> set, T obj) where T : BaseEntity
    {
        if (!set.Any(o => o.Id == obj.Id)) return NotFound();

        obj.UpdatedAt = DateTime.Now;
        context.Entry(obj).State = EntityState.Modified;
        context.Update(obj);
        context.SaveChanges();

        return Ok();
    }

    protected ActionResult Delete<T>(DbContext context, DbSet<T> set, int id) where T : BaseEntity
    {
        var obj = set.Find(id);

        if (obj == null) return NotFound();

        context.Remove(obj);
        context.SaveChanges();

        return Ok();
    }

    protected static Func<Expression<Func<TSource, object>>, IOrderedQueryable<TSource>>
        GetOrderDirection<TSource>(IQueryable<TSource> collection, bool isAscending)
    {
        return isAscending ? collection.OrderBy : collection.OrderByDescending;
    }

    protected static Func<Expression<Func<TSource, object>>, IOrderedQueryable<TSource>>
        GetSecondaryOrderDirection<TSource>(IOrderedQueryable<TSource> collection, bool isAscending)
    {
        return isAscending ? collection.ThenBy : collection.ThenByDescending;
    }
    
    protected static IQueryable<T> ApplyPagination<T>(IQueryable<T> collection,
        int pageSize, int pageNumber)
    {
        if (pageSize <= 0 || pageNumber <= 0) return collection;

        return collection.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }
}
