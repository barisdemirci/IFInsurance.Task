using System;
using IFInsurance.Library;

namespace IFInsurance.ObjectBuilder
{
    public static class RiskBuilder
    {
        public static Risk Build(string name = "name", decimal yearlyPrice = 100)
        {
            Risk risk = new Risk()
            {
                Name = name,
                YearlyPrice = yearlyPrice
            };

            return risk;
        }
    }
}