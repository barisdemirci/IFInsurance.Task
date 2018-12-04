using System;
using System.Collections.Generic;
using System.Linq;
using IFInsurance.Common.Exceptions;
using IFInsurance.Library;
using IFInsurance.Service.Policy;

namespace IFInsurance.Service
{
    public class InsuranceCompanyService : IInsuranceCompany
    {
        private List<IPolicy> policies;
        private readonly IPolicyService policyService;

        public InsuranceCompanyService(IPolicyService policy)
        {
            this.policyService = policy ?? throw new ArgumentNullException(nameof(policy));

            policies = new List<IPolicy>();
            policies.Add(new PolicyA()
            {
                NameOfInsuredObject = "Policy 1",
                ValidFrom = DateTime.UtcNow,
                ValidTill = DateTime.UtcNow.AddMonths(6),
                InsuredRisks = new List<Risk>()
                {
                    new Risk()
                    {
                        Name = "Risk 1",
                        YearlyPrice = 100
                    }
                }
            });
        }

        public string Name => "IF";

        public IList<Risk> AvailableRisks { get; set; }

        public void AddRisk(string nameOfInsuredObject, Risk risk, DateTime validFrom, DateTime effectiveDate)
        {
            if (string.IsNullOrEmpty(nameOfInsuredObject)) throw new ArgumentNullException(nameof(nameOfInsuredObject));
            if (string.IsNullOrEmpty(risk.Name)) throw new ArgumentNullException(nameof(risk.Name));

            if (validFrom.Date < DateTime.UtcNow.Date)
            {
                throw new RiskValidDateException("Valid from can not be in the past");
            }
            bool isRiskAvailable = IsRiskAvailable(risk);
            if (!isRiskAvailable)
            {
                throw new RiskNotAvailableException("Risk is not available");
            }

            var policy = policyService.GetPolicy(nameOfInsuredObject, effectiveDate);
            policy.InsuredRisks.Add(risk);
        }

        public IPolicy GetPolicy(string nameOfInsuredObject, DateTime effectiveDate)
        {
            if (string.IsNullOrEmpty(nameOfInsuredObject)) throw new ArgumentNullException(nameof(nameOfInsuredObject));

            return policyService.GetPolicy(nameOfInsuredObject, effectiveDate);
        }

        public void RemoveRisk(string nameOfInsuredObject, Risk risk, DateTime validTill, DateTime effectiveDate)
        {
            if (string.IsNullOrEmpty(nameOfInsuredObject)) throw new ArgumentNullException(nameof(nameOfInsuredObject));
            if (string.IsNullOrEmpty(risk.Name)) throw new ArgumentNullException(nameof(risk.Name));
            if (effectiveDate.Date > validTill.Date) throw new RiskValidTillException("Must be equal to or greater than date when risk become active");

            var policy = policyService.GetPolicy(nameOfInsuredObject, effectiveDate);
            policy.InsuredRisks.Remove(risk);
        }

        public IPolicy SellPolicy(string nameOfInsuredObject, DateTime validFrom, short validMonths, IList<Risk> selectedRisks)
        {
            if (string.IsNullOrEmpty(nameOfInsuredObject)) throw new ArgumentNullException(nameof(nameOfInsuredObject));

            return policyService.SellPolicy(nameOfInsuredObject, validFrom, validMonths, selectedRisks);
        }

        private bool IsRiskAvailable(Risk risk)
        {
            return AvailableRisks.Contains(risk);
        }
    }
}