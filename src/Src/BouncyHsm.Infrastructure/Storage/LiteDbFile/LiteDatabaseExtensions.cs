using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Storage.LiteDbFile;

internal static class LiteDatabaseExtensions
{
    public static void ExecuteInTransaction(this LiteDatabase database, Action<LiteDatabase> action)
    {
        if (!database.BeginTrans())
        {
            throw new InvalidProgramException("Reentrant transaction is not supported.");
        }

        try
        {
            action.Invoke(database);
            database.Commit();
        }
        catch (Exception)
        {
            database.Rollback();

            throw;
        }
    }

    public static T ExecuteInTransaction<T>(this LiteDatabase database, Func<LiteDatabase, T> function)
    {
        if (!database.BeginTrans())
        {
            throw new InvalidProgramException("Reentrant transaction is not supported.");
        }

        try
        {
            T result = function.Invoke(database);
            database.Commit();

            return result;
        }
        catch (Exception)
        {
            database.Rollback();

            throw;
        }
    }
}
