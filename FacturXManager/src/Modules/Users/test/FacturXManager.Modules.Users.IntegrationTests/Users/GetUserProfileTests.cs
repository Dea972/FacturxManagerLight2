using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FacturXManager.Modules.Users.Application.Users;
using FacturXManager.Modules.Users.Application.Users.GetUser;
using FacturXManager.Modules.Users.IntegrationTests.Abstractions;
using FacturXManager.Modules.Users.Presentation.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace FacturXManager.Modules.Users.IntegrationTests.Users;

public class GetUserProfileTests : BaseIntegrationTest
{
    public GetUserProfileTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_WhenAccessTokenNotProvided()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_ReturnOk_WhenUserExists()
    {
        // Arrange
        string accessToken = await RegisterUserAndGetAccessTokenAsync(Faker.Internet.Email(), Faker.Internet.UserName());

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            accessToken);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        UserResponse? user = await response.Content.ReadFromJsonAsync<UserResponse>();
        user.Should().NotBeNull();
    }

    private async Task<string> RegisterUserAndGetAccessTokenAsync(string email, string userName)
    {
        var request = new RegisterUser.RegisterRequest
        {
            Email = email,
            UserName = userName,
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName()
        };

        await HttpClient.PostAsJsonAsync("api/users/register", request);

        string accessToken = await GetAccessTokenAsync(request.Email, request.UserName);

        return accessToken;
    }
}
