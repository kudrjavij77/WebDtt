using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http.Filters;

namespace WebDtt.Models.Extentions
{
    public class AisDbExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var sb = new StringBuilder();
            var e = actionExecutedContext.Exception;
            while (e.InnerException != null)
            {
                sb.AppendLine(e.InnerException.Message);
                e = e.InnerException;
            }
            var sqlException = e as SqlException;
            if (sqlException == null)
                throw actionExecutedContext.Exception;
            var errNumber = sqlException.Number;
            throw new XDatabaseIntegrityException(new AisDbExceptionBuilder(e.Message, errNumber, sb).HumanMessage);

        }
    }
}