using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;

using NUnit.Framework;

using SteamGuardDesktop.Service.Account;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace SteamGuardDesktop.ArchitectureTests
{
    [TestFixture]
    public sealed class ProductionArchitectureTests
    {
        private static readonly System.Reflection.Assembly ProductionAssembly = typeof(IAccountService).Assembly;

        private static readonly Architecture Architecture = new ArchLoader()
            .LoadAssemblies(ProductionAssembly)
            .Build();

        private static readonly IObjectProvider<IType> DomainTypes = Types()
            .That()
            .ResideInNamespaceMatching("SteamGuardDesktop\\.Domain.*");

        private static readonly IObjectProvider<IType> ServiceTypes = Types()
            .That()
            .ResideInNamespaceMatching("SteamGuardDesktop\\.Service.*");

        private static readonly IObjectProvider<IType> ClientTypes = Types()
            .That()
            .ResideInNamespaceMatching("SteamGuardDesktop\\.Client.*");

        private static readonly IObjectProvider<IType> ViewModelTypes = Types()
            .That()
            .ResideInNamespaceMatching("SteamGuardDesktop\\.ViewModels.*");

        [Test]
        public void GivenTheProductionAssembly_WhenLoaded_ThenItIsNamedSteamGuardDesktop()
            => Assert.That(ProductionAssembly.GetName().Name, Is.EqualTo("SteamGuardDesktop"));

        [Test]
        public void GivenTheProductionAssembly_WhenLoaded_ThenItContainsTheExpectedBoundaries()
        {
            string[] namespaces = ProductionAssembly
                .GetTypes()
                .Select(type => type.Namespace ?? string.Empty)
                .Distinct()
                .ToArray();

            Assert.That(namespaces, Has.Some.StartsWith("SteamGuardDesktop.Domain"));
            Assert.That(namespaces, Has.Some.StartsWith("SteamGuardDesktop.Service"));
            Assert.That(namespaces, Has.Some.StartsWith("SteamGuardDesktop.Client"));
            Assert.That(namespaces, Has.Some.StartsWith("SteamGuardDesktop.ViewModels"));
        }

        [Test]
        public void GivenDomainTypes_WhenChecked_ThenTheyDoNotDependOnServicesClientsOrViewModels()
        {
            IArchRule rule = Types()
                .That()
                .Are(DomainTypes)
                .Should()
                .NotDependOnAny(ServiceTypes)
                .AndShould()
                .NotDependOnAny(ClientTypes)
                .AndShould()
                .NotDependOnAny(ViewModelTypes)
                .As("domain types must not depend on services, clients, or view models");

            rule.Check(Architecture);
        }

        [Test]
        public void GivenServiceTypes_WhenChecked_ThenTheyDoNotDependOnViewModels()
        {
            IArchRule rule = Types()
                .That()
                .Are(ServiceTypes)
                .Should()
                .NotDependOnAny(ViewModelTypes)
                .As("service types must not depend on view models");

            rule.Check(Architecture);
        }

        [Test]
        public void GivenViewModelTypes_WhenChecked_ThenTheyDoNotDependOnHttpClients()
        {
            IArchRule rule = Types()
                .That()
                .Are(ViewModelTypes)
                .Should()
                .NotDependOnAny(ClientTypes)
                .As("view models must not depend on the Steam protocol client");

            rule.Check(Architecture);
        }

        [Test]
        public void GivenDomainTypes_WhenChecked_ThenTheyDoNotUseCryptographyOrHttp()
        {
            IArchRule rule = Types()
                .That()
                .Are(DomainTypes)
                .Should()
                .NotDependOnAny(Types().That().ResideInNamespaceMatching("System\\.Security\\.Cryptography.*"))
                .AndShould()
                .NotDependOnAny(Types().That().ResideInNamespaceMatching("System\\.Net\\.Http.*"))
                .As("domain types must stay free of cryptography and transport concerns");

            rule.Check(Architecture);
        }

        [Test]
        public void GivenTheProductionAssembly_WhenChecked_ThenItDeclaresNoInternalsVisibleTo()
        {
            string[] exposedAssemblies = ProductionAssembly
                .GetCustomAttributes<System.Runtime.CompilerServices.InternalsVisibleToAttribute>()
                .Select(attribute => attribute.AssemblyName)
                .ToArray();

            Assert.That(exposedAssemblies, Is.Empty);
        }
    }
}