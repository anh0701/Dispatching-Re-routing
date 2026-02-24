using System.Data;
using Microsoft.AspNetCore.Mvc;

[ApiController] 
[Route("api/work-centers")]
public class WorkCentersController: ControllerBase
{
    private readonly IDbConnection _db;

    public WorkCentersController(IDbConnection db)
    {
        _db = db;
    }
    
}