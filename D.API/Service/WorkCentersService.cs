using System.Data;
using Dapper;

public class WorkCentersService
{
    private readonly IDbConnection _db;

    public WorkCentersService(IDbConnection db)
    {
        _db = db;
    }

    public async Task<dynamic> GetWorkCenters()
    {
        string sql = "select * from WorkCenters";
        var data = await _db.QueryAsync(sql);
        return data;
    }
    
}