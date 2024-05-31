using System.Net;
using AutoFixture.Xunit2;
using FinanceManager.Account.Domain;
using FinanceManager.Account.Models;
using FinanceManager.Testing.Clients;
using FinanceManager.Testing.Clients.Exceptions;
using FluentAssertions;
using Xunit.Abstractions;
using UriBuilder = System.UriBuilder;

namespace FinanceManager.Account.Tests.ComponentTests;

public class CurrencyTests : AccountTestFixture
{
    private const string BaseUrl = "api/v1/currencies";
    private IRestServiceClient _httpClient = null!;
    private readonly UriBuilder _uriBuilder;

    public CurrencyTests(ITestOutputHelper output) : base(output)
    {
        _uriBuilder = new UriBuilder();
        _uriBuilder.Path = BaseUrl;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _httpClient = await GetRestClient(true);
    }

    [Fact]
    public async Task FirstCall_GetCurrency_ShouldReturnEmpty()
    {
        //Act
        var response = await _httpClient.GetAsync<CurrencyResponse[]>(_uriBuilder.Uri);

        //Assert
        response.Should().NotBeNull();
        response.Length.Should().Be(0);
    }

    [Theory, AutoData]
    public async Task SomeCreateModel_CreateCurrency_ShouldCreate(
        CreateCurrencyModel createModel,
        string requestId)
    {
        //Act
        var response = await _httpClient.PostAsync<CurrencyResponse, CreateCurrencyModel>(_uriBuilder.Uri, createModel, requestId: requestId);

        //Assert
        response.Should().NotBeNull();
        response.ShortName.Should().Be(createModel.ShortName);
        response.Name.Should().Be(createModel.Name);
        response.Icon.Should().Be(createModel.Icon);
        var getResponse = await _httpClient.GetAsync<CurrencyResponse[]>(_uriBuilder.Uri, requestId: requestId);
        getResponse.Should().NotBeNull();
        getResponse.Length.Should().Be(1);
    }

    [Theory, AutoData]
    public async Task MultipleTimesWithTheSameRequestId_CreateCurrency_ShouldThrowRestServiceClientException(
        CreateCurrencyModel createModel,
        string requestId)
    {
        //Arrange
        var createResponse = await _httpClient.PostAsync<CurrencyResponse, CreateCurrencyModel>(_uriBuilder.Uri, createModel, requestId: requestId);
        createResponse.Should().NotBeNull();

        //Act
        Func<Task> action = () => _httpClient.PostAsync<CurrencyResponse, CreateCurrencyModel>(_uriBuilder.Uri, createModel, requestId: requestId);

        //Assert
        await action.Should().ThrowAsync<RestServiceClientException>()
            .Where(x => x.StatusCode == HttpStatusCode.BadRequest);
    }

    [Theory, AutoData]
    public async Task SomeCurrencyCreateModels_BulkCreateCurrencies_ShouldReturnCurrencies(
        CreateCurrencyModel[] createModels,
        long userId)
    {
        //Act
        _uriBuilder.Path += $"/{userId}/bulk";
        var response = await _httpClient.PostAsync<Currency[], CreateCurrencyModel[]>(
            _uriBuilder.Uri,
            createModels,
            apiKey: ApiKey);

        //Assert
        response.Should().NotBeNull();
        response.Length.Should().Be(createModels.Length);
    }

    [Theory, AutoData]
    public async Task SomeCurrency_DeleteCurrency_ShouldBeDeleted(
        CreateCurrencyModel createModel,
        string requestId)
    {
        //Arrange
        var createResponse = await _httpClient.PostAsync<CurrencyResponse, CreateCurrencyModel>(_uriBuilder.Uri, createModel, requestId: requestId);
        createResponse.Should().NotBeNull();
        var currencyId = createResponse.Id;

        //Act
        var deleteUriBuilder = new UriBuilder { Path = $"{BaseUrl}/{currencyId}" };
        await _httpClient.DeleteAsync(deleteUriBuilder.Uri);

        //Assert
        var getResponse = await _httpClient.GetAsync<CurrencyResponse[]>(_uriBuilder.Uri, requestId: requestId);
        getResponse.Should().BeEmpty();
    }

    [Theory, AutoData]
    public async Task SomeCurrency_RejectCurrency_ShouldBeDeleted(
        CreateCurrencyModel createModel,
        string requestId)
    {
        //Arrange
        var createResponse = await _httpClient.PostAsync<CurrencyResponse, CreateCurrencyModel>(_uriBuilder.Uri, createModel, requestId: requestId);
        createResponse.Should().NotBeNull();

        //Act
        await _httpClient.DeleteAsync(_uriBuilder.Uri, requestId: requestId, apiKey: ApiKey);

        //Assert
        var getResponse = await _httpClient.GetAsync<CurrencyResponse[]>(_uriBuilder.Uri, requestId: requestId);
        getResponse.Should().BeEmpty();
    }
}