using System;

using NUnit.Framework;

using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.Service.Time;
using SteamGuardDesktop.UnitTests.Utils;

using TOTPSteamGuard = SteamGuard.TOTP.SteamGuard;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class AuthenticatorCodeExpiryTests
    {
        private const string ValidSharedSecret = "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3";

        [Test]
        public void GivenACodeNearExpiry_WhenEvaluatingItAfterTheWindow_ThenItHasExpired()
        {
            DateTimeOffset moment = DateTimeOffset.FromUnixTimeSeconds(1617591917);
            AuthenticatorViewModelHarness harness = new(moment);

            harness.Refresh(ValidSharedSecret);

            Assert.That(harness.ViewModel.HasExpired(), Is.False);
            Assert.That(harness.ViewModel.RemainingSeconds, Is.EqualTo(13));
        }

        [Test]
        public void GivenACodeNearExpiry_WhenTimePassesBeyondTheWindow_ThenItHasExpired()
        {
            DateTimeOffset moment = DateTimeOffset.FromUnixTimeSeconds(1617591917);
            AuthenticatorViewModelHarness harness = new(moment);

            harness.Refresh(ValidSharedSecret);
            harness.AdvanceTo(moment.AddSeconds(13));

            Assert.That(harness.ViewModel.HasExpired());
        }

        [Test]
        public void GivenACode_WhenTimeAdvancesWithinTheWindow_ThenTheRemainingSecondsDecrease()
        {
            DateTimeOffset moment = DateTimeOffset.FromUnixTimeSeconds(1617591917);
            AuthenticatorViewModelHarness harness = new(moment);

            harness.Refresh(ValidSharedSecret);
            harness.AdvanceTo(moment.AddSeconds(6));
            harness.ViewModel.UpdateCountdown();

            Assert.That(harness.ViewModel.RemainingSeconds, Is.EqualTo(7));
        }

        [Test]
        public void GivenAnInvalidSharedSecret_WhenRefreshing_ThenNoCodeIsDisplayed()
        {
            AuthenticatorViewModelHarness harness = new(DateTimeOffset.FromUnixTimeSeconds(1617591917));

            harness.Refresh("not-base32");

            Assert.That(harness.ViewModel.HasCode, Is.False);
            Assert.That(harness.ViewModel.HasSecretProblem);
        }

        [Test]
        public void GivenAClockDriftReport_WhenApplyingIt_ThenTheWarningIsReflected()
        {
            AuthenticatorViewModelHarness harness = new(DateTimeOffset.FromUnixTimeSeconds(1617591917));

            harness.ViewModel.ApplyClockDrift(new ClockDriftReport { IsSynchronised = false });
            Assert.That(harness.ViewModel.HasClockWarning);

            harness.ViewModel.ApplyClockDrift(new ClockDriftReport { IsSynchronised = true });
            Assert.That(harness.ViewModel.HasClockWarning, Is.False);
        }

        private sealed class AuthenticatorViewModelHarness
        {
            private readonly MutableClock clock;

            public AuthenticatorViewModelHarness(DateTimeOffset moment)
            {
                clock = new MutableClock(moment);

                IOtpCodeGenerator generator = new OtpCodeGenerator(
                    new TOTPSteamGuard(new FixedTimeStepProvider(1L)),
                    clock);

                ViewModel = new SteamGuardDesktop.ViewModels.AuthenticatorViewModel(generator, clock);
            }

            public SteamGuardDesktop.ViewModels.AuthenticatorViewModel ViewModel { get; }

            public void Refresh(string sharedSecret) => ViewModel.Refresh(sharedSecret);

            public void AdvanceTo(DateTimeOffset moment) => clock.SetUtcNow(moment);
        }

        private sealed class MutableClock(DateTimeOffset moment) : IClock
        {
            private DateTimeOffset currentMoment = moment;

            public DateTimeOffset GetUtcNow() => currentMoment;

            public void SetUtcNow(DateTimeOffset moment) => currentMoment = moment;
        }
    }
}
