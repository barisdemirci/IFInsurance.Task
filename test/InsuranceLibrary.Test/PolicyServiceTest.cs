using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using IFInsurance.Common.Exceptions;
using IFInsurance.Library;
using IFInsurance.ObjectBuilder;
using IFInsurance.Service.Policy;
using NSubstitute;
using Xunit;

namespace IFInsurance.Service.Test
{
    public class PolicyServiceTest
    {
        private readonly PolicyService policyService;

        public PolicyServiceTest()
        {
            policyService = new PolicyService();
            // mock data
            Risk risk = new Risk()
            {
                Name = "Risk 1",
                YearlyPrice = 100
            };
            policyService.Policies = new List<IPolicy>();
            policyService.Policies.Add(new PolicyA()
            {
                NameOfInsuredObject = "Policy 1",
                ValidFrom = DateTime.UtcNow,
                ValidTill = DateTime.UtcNow.AddYears(1),
                InsuredRisks = new List<Risk>()
                {
                    risk
                }
            });
            policyService.Policies.Add(new PolicyA()
            {
                NameOfInsuredObject = "Policy 2",
                ValidFrom = DateTime.UtcNow,
                ValidTill = DateTime.UtcNow.AddYears(1),
                InsuredRisks = new List<Risk>()
                {
                    risk
                }
            });
        }

        [Fact]
        public void GetPolicy_NameOfInsuredObjectIsNull_ThrowsArgumentNullException()
        {
            // arrange
            DateTime effectiveDate = DateTime.UtcNow;
            string nameOfInsuredObject = null;

            // act && assert
            Assert.Throws<ArgumentNullException>(() => policyService.GetPolicy(nameOfInsuredObject, effectiveDate));
        }

        [Fact]
        public void GetPolicy_ThrowsPolicyNotFoundException()
        {
            // arrange
            string nameOfInsuredObject = "Policy Not Found";
            DateTime effectiveDate = DateTime.UtcNow;

            // act && assert
            Assert.Throws<PolicyNotFoundException>(() => policyService.GetPolicy(nameOfInsuredObject, effectiveDate));
        }

        [Fact]
        public void GetPolicy_ArgsOk_ReturnsPolicyObject()
        {
            // arrange
            string nameOfInsuredObject = "Policy 2";
            DateTime effectiveDate = DateTime.UtcNow;

            // act
            var result = policyService.GetPolicy(nameOfInsuredObject, effectiveDate);

            // assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void SellPolicy_NameOfInsuredObjectIsNotUnique_ThrowsPolicyUniqueNameException()
        {
            // arrange
            string nameOfInsuredObject = "Policy 1";
            DateTime validFrom = DateTime.UtcNow.AddMonths(1);
            short validMonths = 3;

            // act && assert
            Assert.Throws<PolicyUniqueNameException>(() => policyService.SellPolicy(nameOfInsuredObject, validFrom, validMonths, Arg.Any<IList<Risk>>()));
        }

        [Fact]
        public void SellPolicy_ValidFromIsInThePast_ThrowsPolicyStartDateException()
        {
            // arrange
            DateTime validFrom = DateTime.UtcNow.AddDays(-1);
            string nameOfInsuredObject = "insurance";
            short validMonths = 10;

            // act && assert
            Assert.Throws<PolicyStartDateException>(() => policyService.SellPolicy(nameOfInsuredObject, validFrom, validMonths, Arg.Any<IList<Risk>>()));
        }

        [Fact]
        public void SellPolicy_ArgsOk_ReturnsNewPolicyObject()
        {
            // arrange
            DateTime validFrom = DateTime.UtcNow.AddDays(1);
            string nameOfInsuredObject = "insurance";
            short validMonths = 10;

            // act
            var result = policyService.SellPolicy(nameOfInsuredObject, validFrom, validMonths, Arg.Any<IList<Risk>>());

            // assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void CalculatePremium_ReturnsSumOfRiskYearlyPrice()
        {
            // arrange
            DateTime validFrom = DateTime.UtcNow;
            DateTime validTill = DateTime.UtcNow.AddYears(1);
            string nameOfInsuranceObject = "nameOfInsuranceObject";
            List<Risk> risks = new List<Risk>()
            {
                new Risk() { Name= "Risk 1", YearlyPrice = 100},
                new Risk() { Name= "Risk 2", YearlyPrice = 100}
            };
            PolicyA policy = PolicyABuilder.Build(validFrom, validTill, risks, nameOfInsuranceObject);
            decimal expectedPremium = 200;

            // assert
            policy.Premium.Should().Be(expectedPremium);
        }

        [Fact]
        public void CalculatePremium_NoRisk_ReturnsZero()
        {
            // arrange
            DateTime validFrom = DateTime.UtcNow;
            DateTime validTill = DateTime.UtcNow.AddYears(1);
            string nameOfInsuranceObject = "nameOfInsuranceObject";
            List<Risk> risks = null;
            PolicyA policy = PolicyABuilder.Build(validFrom, validTill, risks, nameOfInsuranceObject);
            decimal expectedPremium = 0;

            // assert
            policy.Premium.Should().Be(expectedPremium);
        }
    }
}
