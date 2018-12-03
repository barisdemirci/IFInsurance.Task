using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using IFInsurance.Common.Exceptions;
using IFInsurance.Library;
using IFInsurance.ObjectBuilder;
using IFInsurance.Service.Policy;
using NSubstitute;
using Xunit;

namespace IFInsurance.Service.Test
{
    public class InsuranceCompanyServiceTest
    {
        // You can update list of available risks at any time.
        // You can sell policy with initial list of risks.
        // You can add or remove risks at any moment within policy period.
        // Premium must be calculated according to risk validity period.
        // There could be several policies with the same insured object name, but different effective date

        InsuranceCompanyService insuranceCompanyService;
        private readonly IPolicyService policyService;

        public static IEnumerable<object[]> SellPolicyParameters
        {
            get
            {
                return new[]
               {
                    new object[] { "Policy 1", DateTime.UtcNow.AddYears(1), 5 }, // valid from is out of range
                    new object[] { "Policy 1", DateTime.UtcNow, 12}, // valid till is out of range
                    new object[] { "Policy 2", DateTime.UtcNow, 2}, // policy name is unique
                };
            }
        }

        public InsuranceCompanyServiceTest()
        {
            policyService = Substitute.For<IPolicyService>();
            insuranceCompanyService = new InsuranceCompanyService(policyService);

            // set availableRisks
            List<Risk> availableRisks = new List<Risk>()
            {
                RiskBuilder.Build("Risk 1", 100),
                RiskBuilder.Build("Risk 2", 10)
            };
            insuranceCompanyService.AvailableRisks = availableRisks;
        }

        [Fact]
        public void UpdateAvailableRisks_ArgsOk_AvailableRisksEqual()
        {
            // arrange
            List<Risk> availableRisks = new List<Risk>()
            {
                new Risk()
                {
                    Name = "Risk 1",
                    YearlyPrice = 100
                }
            };

            // act
            insuranceCompanyService.AvailableRisks = availableRisks;

            // assert
            insuranceCompanyService.AvailableRisks.Should().BeEquivalentTo(availableRisks);
        }

        [Fact]
        public void SellPolicy_NameOfInsuredObjectIsNull_ThrowsException()
        {
            // arrange
            string nameOfInsuredObject = null;
            DateTime validFrom = DateTime.UtcNow.AddMonths(1);
            short validMonths = 3;

            // act && assert
            Assert.Throws<ArgumentNullException>(() => insuranceCompanyService.SellPolicy(nameOfInsuredObject, validFrom, validMonths, Arg.Any<IList<Risk>>()));
        }

        [Fact]
        public void SellPolicy_NameOfInsuredObjectIsNotUnique_ThrowsException()
        {
            // arrange
            string nameOfInsuredObject = "Policy 1";
            DateTime validFrom = DateTime.UtcNow.AddMonths(1);
            short validMonths = 3;

            // act && assert
            Assert.Throws<PolicyUniqueNameException>(() => insuranceCompanyService.SellPolicy(nameOfInsuredObject, validFrom, validMonths, Arg.Any<IList<Risk>>()));
        }

        [Fact]
        public void SellPolicy_ValidFromIsInThePast_ThrowsException()
        {
            // arrange
            DateTime validFrom = DateTime.UtcNow.AddDays(-1);
            string nameOfInsuredObject = "insurance";
            short validMonths = 10;

            // act && assert
            Assert.Throws<PolicyStartDateException>(() => insuranceCompanyService.SellPolicy(nameOfInsuredObject, validFrom, validMonths, Arg.Any<IList<Risk>>()));
        }

        [Theory]
        [MemberData(nameof(SellPolicyParameters))]
        public void SellPolicy_ArgsOk_ReturnsPolicy(string nameOfInsuranceObject, DateTime validFrom, short validMonths)
        {
            // arrange
            List<Risk> selectedRisks = new List<Risk>()
            {
                new Risk()
                {
                    Name = "Risk",
                    YearlyPrice = 100
                }
            };
            DateTime expectedValidTill = validFrom.AddMonths(validMonths);
            PolicyA policy = PolicyABuilder.Build(validFrom, expectedValidTill, insuranceCompanyService.AvailableRisks.ToList(), nameOfInsuranceObject);
            policyService.SellPolicy(nameOfInsuranceObject, validFrom, validMonths, selectedRisks).Returns(policy);

            // act
            var result = insuranceCompanyService.SellPolicy(nameOfInsuranceObject, validFrom, validMonths, selectedRisks);

            // assert
            // it would be better if I have a chance to mock it
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(policy);
        }

        [Theory]
        [InlineData("name of insurance", null)]
        [InlineData(null, "new risk name")]
        public void AddRisk_ArgsAreInvalid_ThrowsException(string nameOfInsuranceObject, string newRiskName)
        {
            // arrange
            Risk risk = new Risk()
            {
                Name = newRiskName,
                YearlyPrice = 50
            };
            DateTime validFrom = DateTime.UtcNow;
            DateTime effectiveDate = DateTime.UtcNow;

            // act && assert
            Assert.Throws<ArgumentNullException>(() => insuranceCompanyService.AddRisk(nameOfInsuranceObject, risk, validFrom, effectiveDate));
        }

        [Fact]
        public void AddRisk_ValidFromIsInThePast_ThrowsException()
        {
            // arrange
            string nameOfInsuranceObject = "name";
            Risk risk = new Risk()
            {
                Name = "New Risk",
                YearlyPrice = 50
            };
            DateTime validFrom = DateTime.UtcNow.AddMonths(-2);
            DateTime effectiveDate = DateTime.UtcNow;

            // act && assert
            Assert.Throws<RiskValidDateException>(() => insuranceCompanyService.AddRisk(nameOfInsuranceObject, risk, validFrom, effectiveDate));
        }

        [Fact]
        public void AddRisk_ArgsAreValid_Success()
        {
            // arrange
            string nameOfInsuranceObject = "Policy 1";
            Risk newRisk = RiskBuilder.Build("New Risk", 50);
            DateTime validFrom = DateTime.UtcNow;
            DateTime effectiveDate = DateTime.UtcNow;
            PolicyA policy = PolicyABuilder.Build(validFrom, effectiveDate, insuranceCompanyService.AvailableRisks.ToList());
            policyService.GetPolicy(nameOfInsuranceObject, effectiveDate).Returns(policy);
            int expectedCountOfRisk = insuranceCompanyService.AvailableRisks.Count + 1;

            // act
            insuranceCompanyService.AddRisk(nameOfInsuranceObject, newRisk, validFrom, effectiveDate);

            // assert
            policy.InsuredRisks.Count.Should().Be(expectedCountOfRisk);
            policy.InsuredRisks.Should().Contain(newRisk);
        }

        [Theory]
        [InlineData("name of insurance", null)]
        [InlineData(null, "risk name")]
        public void RemoveRisk_ArgsAreInvalid_ThrowsException(string nameOfInsuranceObject, string riskName)
        {
            // arrange
            // arrange
            Risk risk = new Risk()
            {
                Name = riskName,
                YearlyPrice = 50
            };
            DateTime validTill = DateTime.UtcNow.AddYears(1);
            DateTime effectiveDate = DateTime.UtcNow;

            // act && assert
            Assert.Throws<ArgumentNullException>(() => insuranceCompanyService.RemoveRisk(nameOfInsuranceObject, risk, validTill, effectiveDate));
        }

        [Fact]
        public void RemoveRisk_ArgsAreValid_Success()
        {
            // arrange
            string nameOfInsuranceObject = "name";
            Risk risk = RiskBuilder.Build("Risk 1", 100);
            DateTime validFrom = DateTime.UtcNow;
            DateTime effectiveDate = DateTime.UtcNow;
            PolicyA policy = PolicyABuilder.Build(validFrom, effectiveDate, insuranceCompanyService.AvailableRisks.ToList(), nameOfInsuranceObject);
            policyService.GetPolicy(nameOfInsuranceObject, effectiveDate).Returns(policy);
            int expectedRiskCount = insuranceCompanyService.AvailableRisks.Count - 1;

            // act
            insuranceCompanyService.RemoveRisk(nameOfInsuranceObject, risk, validFrom, effectiveDate);

            // assert
            policy.InsuredRisks.Count.Should().Be(expectedRiskCount);
            policy.InsuredRisks.Contains(risk).Should().BeFalse();
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