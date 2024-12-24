using CodeBase.EFCore.Data.DB;

namespace CodeBase.EFCore.Data.UnitOfWork;

public class UnitOfWork : UnitOfWorkBase, IUnitOfWork
{
    protected UnitOfWork(IBaseContext context) : base(context)
    {
    }
}