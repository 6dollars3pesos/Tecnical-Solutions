using System;

namespace TecnicalGangplank
{
    public class TechnicalException : Exception
    {
        public TechnicalException() : base("An Error occcured at Tecnical Gangplank")
        {
        }

        public TechnicalException(string errormsg) : base(errormsg)
        {
        }
    }
}