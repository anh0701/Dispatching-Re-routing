using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/test")]
[Tags("01 - API Test")]
public class TestController : ControllerBase
{
    private readonly IDbConnection _db;

    public TestController(IDbConnection db)
    {
        _db = db;
    }

    [HttpGet("start")]
    public async Task<IActionResult> Start()
    {
        var sql = """
            select * from WorkCenters;
        """;
        var data = await _db.QueryAsync(sql);
        return Ok(new {data});
    }

}