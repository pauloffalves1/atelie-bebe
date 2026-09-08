using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Tests.Entities;

public class NewsletterSubscriberTests
{
    private static readonly Email SubscriberEmail = Email.Create("visitante@ateliebebe.com.br");

    [Fact]
    public void Create_Valid_IsActiveByDefault()
    {
        var subscriber = NewsletterSubscriber.Create(SubscriberEmail);

        Assert.Equal(SubscriberEmail, subscriber.Email);
        Assert.True(subscriber.Active);
    }

    [Fact]
    public void Unsubscribe_SetsInactive()
    {
        var subscriber = NewsletterSubscriber.Create(SubscriberEmail);

        subscriber.Unsubscribe();

        Assert.False(subscriber.Active);
    }

    [Fact]
    public void Reactivate_AfterUnsubscribe_SetsActiveAgain()
    {
        var subscriber = NewsletterSubscriber.Create(SubscriberEmail);
        subscriber.Unsubscribe();

        subscriber.Reactivate();

        Assert.True(subscriber.Active);
    }
}
