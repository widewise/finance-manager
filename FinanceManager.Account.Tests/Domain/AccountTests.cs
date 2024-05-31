using AutoFixture;
using AutoFixture.Xunit2;
using FinanceManager.Account.Domain;
using FinanceManager.Events.Models;
using FinanceManager.TransportLibrary.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceManager.Account.Tests.Domain;

public class AccountTests
{
    private readonly DateTime _changeDate;
    private readonly Guid _changeCategoryId;
    private readonly Guid _accountId;
    private readonly string _transactionId;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IMessagePublisher<NotificationSendEvent>> _notificationSendEventPublisherMock;
    private readonly Mock<IMessagePublisher<ChangeStatisticsEvent>> _changeStatisticsEventPublisherMock;

    public AccountTests()
    {
        var fixture = new Fixture();
        _changeDate = fixture.Create<DateTime>();
        _changeCategoryId = Guid.NewGuid();
        _accountId = Guid.NewGuid();
        _transactionId = fixture.Create<string>();

        _loggerMock = new Mock<ILogger>();
        _notificationSendEventPublisherMock = new Mock<IMessagePublisher<NotificationSendEvent>>();
        _changeStatisticsEventPublisherMock = new Mock<IMessagePublisher<ChangeStatisticsEvent>>();
    }

    [AutoData, Theory]
    public async Task NewBalanceLessZeroAndNotEmptyUserAddress_ValidateAndUpdateBalance_ShouldReturnFalseAndSendNotification(
        Account.Domain.Account account,
        AccountLimit[] limits,
        string userAddress)
    {
        //Arrange
        account.Balance = 0;
        //Act
        var result = await account.ValidateAndUpdateBalanceAsync(
            limits,
            -10,
            _changeDate,
            _changeCategoryId,
            _accountId,
            userAddress,
            _transactionId,
            _loggerMock.Object,
            _notificationSendEventPublisherMock.Object,
            _changeStatisticsEventPublisherMock.Object);

        //Assert
        result.Should().BeFalse();
        _notificationSendEventPublisherMock
            .Verify(x => x.SendAsync(It.IsAny<NotificationSendEvent>()),
                Times.Once);
    }

    [AutoData, Theory]
    public async Task NewBalanceLessZeroAndNullUserAddress_ValidateAndUpdateBalance_ShouldReturnFalseAndNotSendNotification(
        Account.Domain.Account account,
        AccountLimit[] limits)
    {
        //Arrange
        account.Balance = 0;
        //Act
        var result = await account.ValidateAndUpdateBalanceAsync(
            limits,
            -10,
            _changeDate,
            _changeCategoryId,
            _accountId,
            null,
            _transactionId,
            _loggerMock.Object,
            _notificationSendEventPublisherMock.Object,
            _changeStatisticsEventPublisherMock.Object);

        //Assert
        result.Should().BeFalse();
        _notificationSendEventPublisherMock
            .Verify(x => x.SendAsync(It.IsAny<NotificationSendEvent>()),
                Times.Never);
    }

    [AutoData, Theory]
    public async Task NewBalanceMoreZeroAndNoLimits_ValidateAndUpdateBalance_ShouldReturnTrueAndSendChangeStatistics(
        Account.Domain.Account account)
    {
        //Arrange
        var changeValue = 10;
        //Act
        var result = await account.ValidateAndUpdateBalanceAsync(
            Array.Empty<AccountLimit>(),
            changeValue,
            _changeDate,
            _changeCategoryId,
            _accountId,
            null,
            _transactionId,
            _loggerMock.Object,
            _notificationSendEventPublisherMock.Object,
            _changeStatisticsEventPublisherMock.Object);

        //Assert
        result.Should().BeTrue();
        _changeStatisticsEventPublisherMock
            .Verify(x => x.SendAsync(It.IsAny<ChangeStatisticsEvent>()),
                Times.Once);
    }

    [AutoData, Theory]
    public async Task NewBalanceMoreZeroAndNotificationLimitAndUserAddress_ValidateAndUpdateBalance_ShouldReturnTrueAndSendBothEvents(
        Account.Domain.Account account,
        AccountLimit notificationLimit,
        string userAddress)
    {
        //Arrange
        var changeValue = 10;
        notificationLimit.LimitValue = 10;
        notificationLimit.Type = AccountLimitType.Notify;
        //Act
        var result = await account.ValidateAndUpdateBalanceAsync(
            new[] { notificationLimit },
            changeValue,
            _changeDate,
            _changeCategoryId,
            _accountId,
            userAddress,
            _transactionId,
            _loggerMock.Object,
            _notificationSendEventPublisherMock.Object,
            _changeStatisticsEventPublisherMock.Object);

        //Assert
        result.Should().BeTrue();
        _notificationSendEventPublisherMock
            .Verify(x => x.SendAsync(It.IsAny<NotificationSendEvent>()),
                Times.Once);
        _changeStatisticsEventPublisherMock
            .Verify(x => x.SendAsync(It.IsAny<ChangeStatisticsEvent>()),
                Times.Once);
    }

    [AutoData, Theory]
    public async Task NewBalanceMoreZeroAndRestrictionLimit_ValidateAndUpdateBalance_ShouldReturnFalseAndNotSendBothEvents(
        Account.Domain.Account account,
        AccountLimit restictionLimit,
        string userAddress)
    {
        //Arrange
        var changeValue = 10;
        restictionLimit.LimitValue = 10;
        restictionLimit.Type = AccountLimitType.Restrict;
        //Act
        var result = await account.ValidateAndUpdateBalanceAsync(
            new[] { restictionLimit },
            changeValue,
            _changeDate,
            _changeCategoryId,
            _accountId,
            userAddress,
            _transactionId,
            _loggerMock.Object,
            _notificationSendEventPublisherMock.Object,
            _changeStatisticsEventPublisherMock.Object);

        //Assert
        result.Should().BeFalse();
        _notificationSendEventPublisherMock
            .Verify(x => x.SendAsync(It.IsAny<NotificationSendEvent>()),
                Times.Never);
        _changeStatisticsEventPublisherMock
            .Verify(x => x.SendAsync(It.IsAny<ChangeStatisticsEvent>()),
                Times.Never);
    }
}