using IFInsurance.Library;
using System;
using System.Collections.Generic;
using System.Text;

namespace IFInsurance.Service.Policy
{
    public interface IPolicyService
    {
        IPolicy GetPolicy(string nameOfInsuredObject, DateTime effectiveDate);

        IPolicy SellPolicy(string nameOfInsuredObject, DateTime validFrom, short validMonths, IList<Risk> selectedRisks);
    }
}