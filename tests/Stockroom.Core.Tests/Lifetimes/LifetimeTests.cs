using Microsoft.Extensions.DependencyInjection;
using Stockroom.Core.Lifetimes;

namespace Stockroom.Core.Tests.Lifetimes;

public class LifetimeTests
{
    [Test]
    public void Lifetimes_GiveSameOrNewInstances()
    {
        var services = new ServiceCollection();
        services.AddSingleton<SingletonMarker>();
        services.AddScoped<ScopedMarker>();
        services.AddTransient<TransientMarker>();
        using var provider = services.BuildServiceProvider();

        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        var a = first.ServiceProvider;
        var b = second.ServiceProvider;

        var singletonA = a.GetRequiredService<SingletonMarker>();
        var singletonB = b.GetRequiredService<SingletonMarker>();
        var scopedA = a.GetRequiredService<ScopedMarker>();
        var scopedAagain = a.GetRequiredService<ScopedMarker>();
        var scopedB = b.GetRequiredService<ScopedMarker>();
        var transient = a.GetRequiredService<TransientMarker>();
        var transientAgain = a.GetRequiredService<TransientMarker>();

        Assert.That(singletonA, Is.SameAs(singletonB));
        Assert.That(scopedA, Is.SameAs(scopedAagain));
        Assert.That(scopedA, Is.Not.SameAs(scopedB));
        Assert.That(transient, Is.Not.SameAs(transientAgain));
    }

    [Test]
    public void CaptiveDependency_IsFoundOnlyByScopeValidation()
    {
        var services = new ServiceCollection();
        services.AddScoped<SignedInUser>();
        services.AddSingleton<ChangeLog>();

        // The same checks the host runs in Development.
        var validating = new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        };
        var error = Assert.Throws<AggregateException>(
            () => services.BuildServiceProvider(validating));
        Assert.That(error.Message,
            Does.Contain("Cannot consume scoped service"));

        // Without the checks, the singleton gets its SignedInUser
        // from the root provider: an instance no scope ever fills.
        using var provider = services.BuildServiceProvider();
        var anna = DescribeChange(provider, "anna");
        var ben = DescribeChange(provider, "ben");

        Assert.That(anna, Is.EqualTo("Price changed by "));
        Assert.That(ben, Is.EqualTo("Price changed by "));
    }

    private static string DescribeChange(
        IServiceProvider provider, string userName)
    {
        using var scope = provider.CreateScope();
        var user = scope.ServiceProvider
            .GetRequiredService<SignedInUser>();
        user.Name = userName;

        return scope.ServiceProvider
            .GetRequiredService<ChangeLog>()
            .Describe("Price changed");
    }
}
