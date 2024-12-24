using Microsoft.EntityFrameworkCore;

namespace CodeBase.EFCore.Data.DbContext;

public class DatabaseContext: Microsoft.EntityFrameworkCore.DbContext, IDbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {

    }

    public void MarkAsModified(object o, string propertyName)
    {
        this.Entry(o).Property(propertyName).IsModified = true;
    }
}