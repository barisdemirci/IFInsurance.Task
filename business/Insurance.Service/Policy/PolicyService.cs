using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IFInsurance.Common.Exceptions;
using IFInsurance.Library;

namespace IFInsurance.Service.Policy
{
    public class PolicyService : IPolicyService
    {
        public List<IPolicy> Policies { get; set; }

        public IPolicy GetPolicy(string nameOfInsuredObject, DateTime effectiveDate)
        {
            if (string.IsNullOrEmpty(nameOfInsuredObject)) throw new ArgumentNullException(nameof(nameOfInsuredObject));

            var policy = Policies.FirstOrDefault(x => x.NameOfInsuredObject == nameOfInsuredObject &&
                                             x.ValidFrom.Date <= effectiveDate.Date &&
                                             x.ValidTill.Date >= effectiveDate.Date);
            if (policy == null)
            {
                throw new PolicyNotFoundException("Policy not found");
            }

            return policy;
        }

        public IPolicy SellPolicy(string nameOfInsuredObject, DateTime validFrom, short validMonths, IList<Risk> selectedRisks)
        {
            if (string.IsNullOrEmpty(nameOfInsuredObject)) throw new ArgumentNullException(nameof(nameOfInsuredObject));

            DateTime validTill = validFrom.AddMonths(validMonths);
            bool isPolicyExist = IsPolicyExist(nameOfInsuredObject, validFrom, validTill);
            if (isPolicyExist)
            {
                throw new PolicyUniqueNameException("Insured object is not unique in the given period.");
            }

            if (validFrom.Date < DateTime.UtcNow.Date)
            {
                throw new PolicyStartDateException("Policy start can not be in the past");
            }

            // In real life, following codes would be usually calling another service/repository
            PolicyA policy = new PolicyA()
            {
                InsuredRisks = selectedRisks,
                NameOfInsuredObject = nameOfInsuredObject,
                ValidFrom = validFrom,
                ValidTill = validTill
            };
            return policy;
        }

        private bool IsPolicyExist(string nameOfInsuredObject, DateTime validFrom, DateTime validTill)
        {
            // In real life, following codes would be usually calling another service/repository
            bool exist = Policies.Exists(x => x.NameOfInsuredObject == nameOfInsuredObject &&
                                             x.ValidFrom.Date <= validFrom.Date &&
                                             x.ValidTill.Date >= validTill.Date);
            return exist;
        }
    }
}