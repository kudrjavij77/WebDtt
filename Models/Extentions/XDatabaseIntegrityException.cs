using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDtt.Models.Extentions
{
    public class XDatabaseIntegrityException : Exception
    {
        public XDatabaseIntegrityException(string message) : base(message)
        {
        }
    }
}