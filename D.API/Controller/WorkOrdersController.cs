using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

[ApiController] 
[Route("api/work-orders")]
public class WorkOrdersController : ControllerBase
{
    private readonly IDbConnection _db;

    public WorkOrdersController(IDbConnection db)
    {
        _db = db;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPlanned ()
    {
        string sql = "select * from WorkOrders where Status = 0";
        var data = await _db.QueryAsync(sql);
        return Ok(new {data});
    }

    [HttpGet("/{id}/route-details")]
    public async Task<IActionResult> GetRouteStep()
    {
        var sql = """
        
        """;
        var data = await _db.QueryAsync(sql);
        return Ok(new {data});
    }
}