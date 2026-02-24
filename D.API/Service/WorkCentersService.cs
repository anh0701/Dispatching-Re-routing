using System.Data;

public class WorkCentersService
{
    private readonly IDbConnection _db;

    public WorkCentersService(IDbConnection db)
    {
        _db = db;
    }
    
}