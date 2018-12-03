using System;
using System.Collections.Generic;
using System.Text;

namespace IFInsurance.Common.Exceptions
{
    public class PolicyStartDateException : Exception
    {
        public PolicyStartDateException()
        {
        }

        public PolicyStartDateException(string message)
            : base(message)
        {

        }
    }
}