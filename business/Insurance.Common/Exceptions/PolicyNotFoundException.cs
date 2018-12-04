using System;

namespace IFInsurance.Common.Exceptions
{
    public class PolicyNotFoundException : Exception
    {
        public PolicyNotFoundException()
        {
        }

        public PolicyNotFoundException(string message)
            : base(message)
        {

        }
    }
}