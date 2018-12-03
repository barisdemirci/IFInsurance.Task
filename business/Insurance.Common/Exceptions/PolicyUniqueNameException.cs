using System;
using System.Collections.Generic;
using System.Text;

namespace IFInsurance.Common.Exceptions
{
    public class PolicyUniqueNameException : Exception
    {
        public PolicyUniqueNameException()
        {
        }

        public PolicyUniqueNameException(string message)
            : base(message)
        {

        }
    }
}