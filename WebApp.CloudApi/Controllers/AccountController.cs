using WebApp.CloudApi.Class;
using WebApp.CloudApi.EfCore;
using WebApp.CloudApi.Model;
using WebApp.CloudApi.RequestModel;
using Microsoft.AspNetCore.Mvc;
using WebApp.CloudApi.Helper;

namespace WebApp.CloudApi.Controllers;

/// <summary>
/// Account controller
/// </summary>
[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IDbHelper _db;
    private readonly ApplicationInstance _application;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
    IDbHelper db,
    ApplicationInstance application, ILogger<AccountController> logger)
    {
        _db = db;
        this._application = application;
        _logger = logger;
    }

    [BasicAuthorization]
    [HttpGet("{id}")]
    public IActionResult Get(Guid id)
    {
        if (this._application.Application != id) {
            return Unauthorized();
        }
        return Ok(_db.GetAccount(id));
    }

    [HttpPost(Name = "account")]
    public async Task<IActionResult> Post([FromBody] AccountRequest account)
    {
        _logger.LogInformation("Create Account called");
        if (ModelState.IsValid)
        {
            string connectionString = $"Host={GlobalData.Application.Where(s => s.Key == "Database").FirstOrDefault().Value};Database={GlobalData.Application.Where(s => s.Key == "DatabaseName").FirstOrDefault().Value};Port={GlobalData.Application.Where(s => s.Key == "DatabasePort").FirstOrDefault().Value};Username={GlobalData.Application.Where(s => s.Key == "MasterUsername").FirstOrDefault().Value};Password={GlobalData.Application.Where(s => s.Key == "MasterPassword").FirstOrDefault().Value};";
             _logger.LogInformation(connectionString);
            try
            {
                var existingAccount = _db.GetAccount(account.Email);
                if (existingAccount.Email == account.Email)
                {
                    return BadRequest("Email already exist");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, eventId:0, ex, ex.Message);
            }
             _logger.LogInformation(" Account Created");
            return Created("", await _db.SaveAccount(account));
        }
        else
        {
            return BadRequest();
        }
    }


    [BasicAuthorization]
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] AccountRequest model)
    {
        return Created("", await _db.UpdateAccount(id, model));
    }
}