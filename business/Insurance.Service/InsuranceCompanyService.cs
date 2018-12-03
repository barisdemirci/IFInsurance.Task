using System;
using System.Collections.Generic;
using System.Linq;
using IFInsurance.Common.Exceptions;
using IFInsurance.Library;

namespace IFInsurance.Service
{
    public class InsuranceCompanyService : IInsuranceCompany
    {
        private List<IPolicy> policies;
        private readonly IPolicy policy;

        public InsuranceCompanyService(IPolicy policy)
        {
            this.policy = policy ?? throw new ArgumentNullException(nameof(policy));

            policies = new List<IPolicy>();
            policies.Add(new Policy()
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

            var policy = GetPolicy(nameOfInsuredObject, effectiveDate);
            policy.InsuredRisks.Add(risk);
        }

        public IPolicy GetPolicy(string nameOfInsuredObject, DateTime effectiveDate)
        {
            if (string.IsNullOrEmpty(nameOfInsuredObject)) throw new ArgumentNullException(nameof(nameOfInsuredObject));

            List<IPolicy> policies = new List<IPolicy>();
            policies.Add(new Policy()
            {
                NameOfInsuredObject = "Policy 1",
                ValidFrom = DateTime.UtcNow,
                ValidTill = DateTime.UtcNow.AddYears(1),
                InsuredRisks = new List<Risk>()
                {
                    new Risk()
                    {
                        Name = "Risk 1",
                        YearlyPrice = 100
                    }
                }
            });
            return policies.SingleOrDefault(x => x.NameOfInsuredObject == nameOfInsuredObject &&
                                            x.ValidFrom.Date <= effectiveDate.Date &&
                                            x.ValidTill.Date >= effectiveDate.Date);
        }

        public void RemoveRisk(string nameOfInsuredObject, Risk risk, DateTime validTill, DateTime effectiveDate)
        {
            if (string.IsNullOrEmpty(nameOfInsuredObject)) throw new ArgumentNullException(nameof(nameOfInsuredObject));
            if (string.IsNullOrEmpty(risk.Name)) throw new ArgumentNullException(nameof(risk.Name));

            policy.InsuredRisks.Remove(risk);
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
            Policy policy = new Policy()
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
            bool exist = policies.Exists(x => x.NameOfInsuredObject == nameOfInsuredObject &&
                                             x.ValidFrom.Date <= validFrom.Date &&
                                             x.ValidTill.Date >= validTill.Date);
            return exist;
        }
    }
}