using System.Data.Common;

#if NET_CORE
using Microsoft.EntityFrameworkCore;// TODO
#else
using System.Data.Entity.Infrastructure.Interception;

namespace Bitbird.Core.Data.DbContext.Interceptors
{
    public class ArithAbortOnInterceptor : IDbCommandInterceptor
    {
        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            command.CommandText = "SET ARITHABORT ON; " + command.CommandText;
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            command.CommandText = "SET ARITHABORT ON; " + command.CommandText;
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {

        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            command.CommandText = "SET ARITHABORT ON; " + command.CommandText;
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }
    }
}
#endif

// TODO
/*
namespace Bitbird.Core.Data.DbContext.Interceptors
{
    public class ArithAbortOnInterceptor : IDbCommandInterceptor
    {
        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            command.CommandText = "SET ARITHABORT ON; " + command.CommandText;
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            command.CommandText = "SET ARITHABORT ON; " + command.CommandText;
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {

        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            command.CommandText = "SET ARITHABORT ON; " + command.CommandText;
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }
    }
}*/
