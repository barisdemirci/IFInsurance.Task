using System;
using System.Collections.Generic;
using System.Text;

namespace IFInsurance.Common.Exceptions
{
    public class RiskNotAvailableException : Exception
    {
        public RiskNotAvailableException()
        {
        }

        public RiskNotAvailableException(string message)
            : base(message)
        {

        }
    }
}