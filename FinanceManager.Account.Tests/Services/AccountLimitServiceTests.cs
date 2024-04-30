using AutoFixture.Xunit2;
using FinanceManager.Account.Domain;
using FinanceManager.Account.Models;
using FinanceManager.Account.Repositories;
using FinanceManager.Account.Services;
using FinanceManager.UnitOfWork.EntityFramework.Abstracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceManager.Account.Tests.Services;

public class AccountLimitServiceTests
{
    private readonly Mock<IAccountLimitRepository> _accountLimitRepositoryMock;
    private readonly Mock<IUnitOfWorkExecuter> _unitOfWorkExecuterMock;
    private readonly AccountLimitService _service;
    public AccountLimitServiceTests()
    {
        _accountLimitRepositoryMock = new Mock<IAccountLimitRepository>();
        _unitOfWorkExecuterMock = new Mock<IUnitOfWorkExecuter>();

        _service = new AccountLimitService(
            Mock.Of<ILogger<AccountLimitService>>(),
            _accountLimitRepositoryMock.Object,
            _unitOfWorkExecuterMock.Object);
    }

    [AutoData, Theory]
    public async Task DataFromRepository_GetAsync_ShouldReturnData(AccountLimit[] limits)
    {
        //Arrange
        _accountLimitRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<AccountLimitSpecification>()))
            .ReturnsAsync(limits);
        
        //Act
        var result = await _service.GetAsync(Guid.NewGuid());

        //Assert
        result.Should().BeEquivalentTo(limits);
    }

    [AutoData, Theory]
    public async Task EntityAlreadyExist_CreateAsync_ShouldReturnNull(
        CreateAccountLimitModel model,
        string requestId)
    {
        //Arrange
        _accountLimitRepositoryMock
            .Setup(x => x.CheckExistAsync(It.IsAny<AccountLimitSpecification>()))
            .ReturnsAsync(true);
        
        //Act
        var result = await _service.CreateAsync(model, requestId);

        //Assert
        result.Should().BeNull();
    }

    [AutoData, Theory]
    public async Task EntityNotExist_CreateAsync_ShouldReturnData(
        CreateAccountLimitModel model,
        string requestId,
        AccountLimit expectedResult)
    {
        //Arrange
        _accountLimitRepositoryMock
            .Setup(x => x.CheckExistAsync(It.IsAny<AccountLimitSpecification>()))
            .ReturnsAsync(false);
        _unitOfWorkExecuterMock
            .Setup(x => x.ExecuteAsync(It.IsAny<Func<IAccountLimitRepository, Task<AccountLimit>>>()))
            .ReturnsAsync(expectedResult);
        
        //Act
        var result = await _service.CreateAsync(model, requestId);

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [AutoData, Theory]
    public async Task EntityNotExist_UpdateAsync_ShouldReturnFalse(
        UpdateAccountLimitModel model)
    {
        //Arrange
        _accountLimitRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<AccountLimitSpecification>()))
            .ReturnsAsync(Array.Empty<AccountLimit>());
        
        //Act
        var result = await _service.UpdateAsync(Guid.NewGuid(),  model);

        //Assert
        result.Should().BeFalse();
    }
    
    [AutoData, Theory]
    public async Task SomeAccountLimit_UpdateAsync_ShouldReturnTrue(
        UpdateAccountLimitModel model,
        AccountLimit accountLimit)
    {
        //Arrange
        _accountLimitRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<AccountLimitSpecification>()))
            .ReturnsAsync(new[] { accountLimit });
        
        //Act
        var result = await _service.UpdateAsync(Guid.NewGuid(),  model);

        //Assert
        result.Should().BeTrue();
        _unitOfWorkExecuterMock.Verify(x =>
            x.ExecuteAsync(It.IsAny<Func<IAccountLimitRepository, Task>>()), Times.Once);
    }
    
    [Fact]
    public async Task EntityNotExist_DeleteAsync_ShouldReturnFalse()
    {
        //Arrange
        _accountLimitRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<AccountLimitSpecification>()))
            .ReturnsAsync(Array.Empty<AccountLimit>());
        
        //Act
        var result = await _service.DeleteAsync(Guid.NewGuid());

        //Assert
        result.Should().BeFalse();
    }
    
    [AutoData, Theory]
    public async Task SomeAccountLimit_DeleteAsync_ShouldReturnTrue(
        AccountLimit accountLimit)
    {
        //Arrange
        _accountLimitRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<AccountLimitSpecification>()))
            .ReturnsAsync(new[] { accountLimit });
        
        //Act
        var result = await _service.DeleteAsync(Guid.NewGuid());

        //Assert
        result.Should().BeTrue();
        _unitOfWorkExecuterMock.Verify(x =>
            x.ExecuteAsync(It.IsAny<Func<IAccountLimitRepository, Task>>()), Times.Once);
    }
}