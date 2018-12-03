using System;
using System.Collections.Generic;
using System.Text;

namespace IFInsurance.Common.Exceptions
{
    public class RiskValidTillException : Exception
    {
        public RiskValidTillException()
        {
        }

        public RiskValidTillException(string message)
            : base(message)
        {

        }
    }
}