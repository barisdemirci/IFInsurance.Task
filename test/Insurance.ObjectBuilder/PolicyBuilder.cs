using System;
using System.Collections.Generic;
using IFInsurance.Service;
using IFInsurance.Library;

namespace IFInsurance.ObjectBuilder
{
    public static class PolicyBuilder
    {
        public static Policy Build(DateTime validFrom, DateTime validTill, List<Risk> insuredRisks, string nameOfInsuredObject = "insurance")
        {
            Policy policy = new Policy()
            {
                NameOfInsuredObject = nameOfInsuredObject,
                ValidFrom = validFrom,
                ValidTill = validTill,
                InsuredRisks = insuredRisks
            };

            return policy;
        }
    }
}