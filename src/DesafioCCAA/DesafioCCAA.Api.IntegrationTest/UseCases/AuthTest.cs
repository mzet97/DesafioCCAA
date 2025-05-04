using Bogus;
using DesafioCCAA.Application.UseCases.Auth.Commands;
using DesafioCCAA.Application.UseCases.Auth.ViewModels;
using DesafioCCAA.Shared.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace DesafioCCAA.Api.IntegrationTest.UseCases;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
}

[CollectionDefinition("ApiIntegrationTest")]
public class ApiIntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory> { }

[Collection("ApiIntegrationTest")]
public class AuthTest
{
    private readonly CustomWebApplicationFactory _factory;

    private readonly string hostApi = "https://localhost:5120/api/";

    private readonly Faker _faker;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AuthTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _faker = new Faker("pt_BR");
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact(DisplayName = "Register user with success")]
    [Trait("AuthControllerTest", "Auth Controller Tests")]
    public async Task RegisterWithSuccess()
    {
        // Arrange
        var viewModel = new RegisterUserCommand(
            _faker.Internet.Email(),
            _faker.Name.FullName(),
            "Admin@123",
            _faker.Date.Past(18, DateTime.Now)
        );

        // Act
        var client = _factory.CreateClient();
        var response = await client.PostAsync($"{hostApi}auth/register",
            new StringContent(JsonSerializer.Serialize(viewModel, _jsonSerializerOptions), System.Text.Encoding.UTF8, "application/json"));
        string json = await response.Content.ReadAsStringAsync();

        var userData = JsonSerializer.Deserialize<BaseResult<LoginResponseViewModel>>(json, _jsonSerializerOptions);


        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(userData);
        Assert.True(userData.Success, $"API returned error: {userData?.Message}");
        Assert.NotNull(userData.Data);
        Assert.Equal(viewModel.Email, userData?.Data.UserToken.Email);
        Assert.Equal(viewModel.Name, userData?.Data.UserToken.Name);
        Assert.Equal(viewModel.BirthDate.Date, userData?.Data.UserToken.BirthDate.Date);
        Assert.True(userData?.Data.ExpiresIn > 0);
        Assert.False(string.IsNullOrWhiteSpace(userData?.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(userData?.Data.UserToken.Id));
        Assert.NotNull(userData?.Data.UserToken.Claims);
        Assert.True(userData?.Data.UserToken.Claims.ToList().Count > 0);
    }

    [Fact(DisplayName = "Register and Login user with success")]
    [Trait("AuthControllerTest", "Auth Controller Tests")]
    public async Task RegisterAndLoginWithSuccess()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var name = _faker.Name.FullName();
        var password = "Admin@123";
        var birthDate = _faker.Date.Past(18, DateTime.Now);

        var registerCommand = new RegisterUserCommand(
            email,
            name,
            password,
            birthDate
        );

        var client = _factory.CreateClient();

        // Register
        var registerResponse = await client.PostAsync($"{hostApi}auth/register",
            new StringContent(JsonSerializer.Serialize(registerCommand, _jsonSerializerOptions), System.Text.Encoding.UTF8, "application/json"));
        string registerJson = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<BaseResult<LoginResponseViewModel>>(registerJson, _jsonSerializerOptions);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        Assert.NotNull(registerResult);
        Assert.True(registerResult.Success, $"Register API error: {registerResult?.Message}");
        Assert.NotNull(registerResult.Data);

        // Login
        var loginCommand = new LoginUserCommand
        {
            Email = email,
            Password = password
        };

        var loginResponse = await client.PostAsync($"{hostApi}auth/login",
            new StringContent(JsonSerializer.Serialize(loginCommand, _jsonSerializerOptions), System.Text.Encoding.UTF8, "application/json"));
        string loginJson = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<BaseResult<LoginResponseViewModel>>(loginJson, _jsonSerializerOptions);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.NotNull(loginResult);
        Assert.True(loginResult.Success, $"Login API error: {loginResult?.Message}");
        Assert.NotNull(loginResult.Data);
        Assert.Equal(email, loginResult?.Data.UserToken.Email);
        Assert.Equal(name, loginResult?.Data.UserToken.Name);
        Assert.Equal(birthDate.Date, loginResult?.Data.UserToken.BirthDate.Date);
        Assert.True(loginResult?.Data.ExpiresIn > 0);
        Assert.False(string.IsNullOrWhiteSpace(loginResult?.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(loginResult?.Data.UserToken.Id));
        Assert.NotNull(loginResult?.Data.UserToken.Claims);
        Assert.True(loginResult?.Data.UserToken.Claims.ToList().Count > 0);
    }
}
