using CodeBase.EFCore.Data.DB;

namespace CodeBase.EFCore.Data.UnitOfWork;

public class UnitOfWorkBase : IUnitOfWorkBase
{
    /// <summary>
    /// true means dbContext was disposed
    /// </summary>
    protected bool Disposed;

    /// <summary>
    /// The DbContext
    /// </summary>
    protected readonly IBaseContext DbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWorkBase"/> class.
    /// </summary>
    /// <param name="context">object context</param>
    protected UnitOfWorkBase(IBaseContext context)
    {
        this.DbContext = context;
    }

    ~UnitOfWorkBase()
    {
        this.Dispose(false);
    }

    /// <inheritdoc />
    public async Task<int> CommitAsync()
    {
        // Save changes with the default options
        return await Task.FromResult(this.DbContext.SaveChanges());
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(true);
    }
    private void Dispose(bool disposing)
    {
        if (this.Disposed)
        {
            return;
        }

        this.DbContext.Dispose();
        this.Disposed = true;

        if (!disposing)
        {
            return;
        }
    }
}