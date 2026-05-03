using System.Net;
using DataLayer;
using DataLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ServiceApi.Controllers;

public class AccountController : BaseController
{
    [HttpPost]
    [Route("EnsureUser")]
    public ActionResult EnsureUser()
    {
        StatusCodeResult result = Ok();
        using var context = new NotebookContext();
        var user = context.Users.FirstOrDefault(u => u.Email == CurrentUserEmail);

        if (user == null)
        {
            context.Users.Add(new User { Email = CurrentUserEmail });
            result = new StatusCodeResult((int)HttpStatusCode.Created);
        }
        else
        {
            context.Users.Entry(user).State = EntityState.Modified;
        }

        context.SaveChanges();

        return result;
    }
}
