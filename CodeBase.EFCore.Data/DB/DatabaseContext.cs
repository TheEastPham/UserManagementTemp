using Microsoft.EntityFrameworkCore;

namespace CodeBase.EFCore.Data.DB;

public class DatabaseContext : DbContext, IBaseContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }
    
    public void MarkAsModified(object o, string propertyName)
    {
        this.Entry(o).Property(propertyName).IsModified = true;
    }
}