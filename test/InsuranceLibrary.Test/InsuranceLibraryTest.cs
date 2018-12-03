using System;
using System.Collections.Generic;
using FluentAssertions;
using IFInsurance.Common.Exceptions;
using IFInsurance.Library;
using IFInsurance.ObjectBuilder;
using IFInsurance.Service;
using NSubstitute;
using Xunit;

namespace InsuranceLibrary.Test
{
    public class InsuranceLibraryTest
    {
        // You can update list of available risks at any time.
        // You can sell policy with initial list of risks.
        // You can add or remove risks at any moment within policy period.
        // Premium must be calculated according to risk validity period.
        // There could be several policies with the same insured object name, but different effective date

        InsuranceCompanyService insuranceCompanyService;
        private readonly IPolicy policy;

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

        public InsuranceLibraryTest()
        {
            policy = Substitute.For<IPolicy>();
            insuranceCompanyService = new InsuranceCompanyService(policy);
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

            // act
            var result = insuranceCompanyService.SellPolicy(nameOfInsuranceObject, validFrom, validMonths, selectedRisks);

            // assert
            // it would be better if I have a chance to mock it
            result.Should().NotBeNull();
            result.InsuredRisks.Should().BeEquivalentTo(selectedRisks);
            result.ValidFrom.Should().Be(validFrom);
            result.ValidTill.Should().Be(expectedValidTill);
            result.NameOfInsuredObject.Should().Be(nameOfInsuranceObject);
        }

        [Theory]
        [InlineData("name of insurance", null)]
        [InlineData(null, "new risk name")]
        public void AddRisk_NameOfInsuranceObjectIsNull_ThrowsException(string nameOfInsuranceObject, string newRiskName)
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
            Risk risk = new Risk()
            {
                Name = "New Risk",
                YearlyPrice = 50
            };
            DateTime validFrom = DateTime.UtcNow;
            DateTime effectiveDate = DateTime.UtcNow;
            List<Risk> availableRisks = new List<Risk>()
            {
                new Risk()
                {
                    Name = "Risk",
                    YearlyPrice = 100
                }
            };
            insuranceCompanyService.AvailableRisks = availableRisks;
            policy.InsuredRisks = new List<Risk>();

            // act
            insuranceCompanyService.AddRisk(nameOfInsuranceObject, risk, validFrom, effectiveDate);

            // assert
            policy.InsuredRisks.Count.Should().Be(1);
            policy.InsuredRisks.Should().Contain(risk);
        }

        [Theory]
        [InlineData("name of insurance", null)]
        [InlineData(null, "risk name")]
        public void RemoveRisk(string nameOfInsuranceObject, string riskName)
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
            Risk risk = new Risk()
            {
                Name = "New Risk",
                YearlyPrice = 50
            };
            DateTime validFrom = DateTime.UtcNow;
            DateTime effectiveDate = DateTime.UtcNow;
            List<Risk> availableRisks = new List<Risk>()
            {
                new Risk()
                {
                    Name = "Risk",
                    YearlyPrice = 100
                }
            };
            insuranceCompanyService.AvailableRisks = availableRisks;

            // act
            insuranceCompanyService.RemoveRisk(nameOfInsuranceObject, risk, validFrom, effectiveDate);

            // assert
            insuranceCompanyService.AvailableRisks.Count.Should().Be(0);
            insuranceCompanyService.AvailableRisks.Should().Contain(risk);
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
            Policy policy = PolicyBuilder.Build(validFrom, validTill, risks, nameOfInsuranceObject);
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
            Policy policy = PolicyBuilder.Build(validFrom, validTill, risks, nameOfInsuranceObject);
            decimal expectedPremium = 0;

            // assert
            policy.Premium.Should().Be(expectedPremium);
        }
    }
}