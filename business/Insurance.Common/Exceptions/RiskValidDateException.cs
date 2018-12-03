using System;
using System.Collections.Generic;
using System.Text;

namespace IFInsurance.Common.Exceptions
{
    public class RiskValidDateException : Exception
    {
        public RiskValidDateException()
        {
        }

        public RiskValidDateException(string message)
            : base(message)
        {

        }
    }
}