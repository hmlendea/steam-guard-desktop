using NUnit.Framework;

using SteamGuardDesktop.Domain.Confirmations;
using SteamGuardDesktop.Service.Confirmations;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class ConfirmationDecisionTests
    {
        [Test]
        public void GivenAnApprovalDecision_WhenReadingTheOperationName_ThenTheSteamAllowOperationIsReturned()
        {
            ConfirmationDecision decision = new() { DecisionType = ConfirmationDecisionType.Approve };

            Assert.That(decision.OperationName, Is.EqualTo("allow"));
        }

        [Test]
        public void GivenADenialDecision_WhenReadingTheOperationName_ThenTheSteamCancelOperationIsReturned()
        {
            ConfirmationDecision decision = new() { DecisionType = ConfirmationDecisionType.Deny };

            Assert.That(decision.OperationName, Is.EqualTo("cancel"));
        }

        [TestCase(1, ConfirmationType.Test)]
        [TestCase(2, ConfirmationType.Trade)]
        [TestCase(3, ConfirmationType.MarketSell)]
        [TestCase(9, ConfirmationType.ApiKeyCreation)]
        [TestCase(11, ConfirmationType.JoinSteamFamily)]
        [TestCase(613, ConfirmationType.Unsupported)]
        [TestCase(0, ConfirmationType.Unsupported)]
        public void GivenAReportedConfirmationType_WhenMappingIt_ThenTheMatchingDomainTypeIsProduced(
            int reportedType,
            ConfirmationType expectedType)
        {
            Assert.That(SteamConfirmationType.FromReportedType(reportedType), Is.EqualTo(expectedType));
        }

        [TestCase(613)]
        [TestCase(0)]
        public void GivenAnUnsupportedConfirmationType_WhenCheckingWhetherItCanBeDecided_ThenItCannotBeDecided(
            int reportedType)
        {
            Confirmation confirmation = new()
            {
                Type = SteamConfirmationType.FromReportedType(reportedType)
            };

            Assert.That(confirmation.CanBeDecided, Is.False);
        }

        [TestCase(2)]
        [TestCase(6)]
        public void GivenASupportedConfirmationType_WhenCheckingWhetherItCanBeDecided_ThenItCanBeDecided(
            int reportedType)
        {
            Confirmation confirmation = new()
            {
                Type = SteamConfirmationType.FromReportedType(reportedType)
            };

            Assert.That(confirmation.CanBeDecided);
        }
    }
}
