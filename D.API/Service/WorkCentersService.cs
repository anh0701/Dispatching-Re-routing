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

    public async Task<bool> ReportBreakdown(int id)
    {
        string sql = """
            UPDATE WorkCenters
            SET Status = 2
            WHERE Id = @id
        """;
        var data = await _db.ExecuteAsync(sql, id) > 0;
        return data;
    }
    
}