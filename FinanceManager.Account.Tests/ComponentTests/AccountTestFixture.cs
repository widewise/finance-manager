using FinanceManager.Testing;
using Xunit.Abstractions;

namespace FinanceManager.Account.Tests.ComponentTests;

public class AccountTestFixture : TestFixture
{
    protected override string AuthScope => "account";

    protected AccountTestFixture(ITestOutputHelper output) : base(output) { }

    protected override async Task StartAsync()
    {
        await ExtEnvironment.Start<Program>(AuthPort, AuthUrl, AuthSecurityKey, ApiKey);
    }
}