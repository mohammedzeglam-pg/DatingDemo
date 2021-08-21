using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers
{

  public class BuggyController : BaseApiController
  {
    private readonly DataContext _context;

    public BuggyController(DataContext context)
    {
      _context = context;
    }

    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret()
    {
      return "secret key";
    }

    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
      AppUser thing = _context.Users.Find(-1);
      return thing != null ? Ok(thing) : NotFound();
    }

    [HttpGet("server-error")]
    public ActionResult<string> GetServerError()
    {
      AppUser thing = _context.Users.Find(-1);
      string thingToReturn = thing.ToString();
      return thingToReturn;
    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
      return BadRequest("this is not ok request");
    }
  }
}
