using System;
using System.Collections.Generic;
using System.Linq;

namespace IFInsurance.Library
{
    public class PolicyA : IPolicy
    {
        public string NameOfInsuredObject { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTill { get; set; }
        public decimal Premium
        {
            get
            {
                /// Take into account that risk price is given for 1 full year. Policy/risk period can be shorter.
                decimal premium = 0;
                if (InsuredRisks != null && InsuredRisks.Any())
                {
                    foreach (var risk in InsuredRisks)
                    {
                        decimal dailyPrice = risk.YearlyPrice / 365;
                        TimeSpan timeSpan = ValidTill - ValidFrom;
                        premium += dailyPrice * timeSpan.Days;
                    }
                }
                return Math.Round(premium, 2);
            }
        }
        public IList<Risk> InsuredRisks { get; set; }
    }
}