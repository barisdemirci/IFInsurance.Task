using System;
using System.Collections.Generic;
using IFInsurance.Library;

namespace IFInsurance.ObjectBuilder
{
    public static class PolicyABuilder
    {
        public static PolicyA Build(DateTime validFrom, DateTime validTill, List<Risk> insuredRisks, string nameOfInsuredObject = "insurance")
        {
            PolicyA policy = new PolicyA()
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