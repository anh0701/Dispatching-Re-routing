using System.Data;
using Microsoft.AspNetCore.Mvc;

[ApiController] 
[Route("api/dispatching-board")]
public class DispatchingBoardController: ControllerBase
{
    private readonly IDbConnection _db;

    public DispatchingBoardController(IDbConnection db)
    {
        _db = db;
    }
    
}