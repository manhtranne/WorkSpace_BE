using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Net;
using WorkSpace.WebApi.Controllers;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.DTOs.Account;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Exceptions;
using FluentAssertions;

namespace WorkSpace.Tests.Controllers
{
	[TestFixture]
	public class AccountControllerTests
	{
		private Mock<IAccountService> _mockAccountService;
		private AccountController _controller;

		[SetUp]
		public void Setup()
		{
			_mockAccountService = new Mock<IAccountService>();
			_controller = new AccountController(_mockAccountService.Object);

			// Provide a HttpContext so controller.GenerateIPAddress() won't throw NullReferenceException
			var httpContext = new DefaultHttpContext();
			httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = httpContext
			};
		}

		[Test]
		public async Task Authenticate_WithValidCredentials_ShouldReturnOkResult()
		{
			// Arrange
			var authRequest = new AuthenticationRequest
			{
				Email = "test@example.com",
				Password = "123456"
			};

			var authResponse = new AuthenticationResponse
			{
				Id = "1",
				Email = "test@example.com",
				UserName = "testuser",
				JWToken = "fake-jwt-token",
				Roles = new System.Collections.Generic.List<string> { "Customer" },
				IsVerified = true,
				RefreshToken = "refresh-token"
			};

			var serviceResponse = new Response<AuthenticationResponse>(authResponse);

			_mockAccountService
				.Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticationRequest>(), It.IsAny<string>()))
				.ReturnsAsync(serviceResponse);

			// Act
			var result = await _controller.AuthenticateAsync(authRequest);

			// Assert
			result.Should().BeOfType<OkObjectResult>();
			var okResult = result as OkObjectResult;
			okResult!.Value.Should().BeEquivalentTo(authResponse);
		}

		[Test]
		public void Authenticate_WithInvalidCredentials_ShouldThrowApiException()
		{
			// Arrange
			var authRequest = new AuthenticationRequest
			{
				Email = "wrong@example.com",
				Password = "wrongpass"
			};

			_mockAccountService
				.Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticationRequest>(), It.IsAny<string>()))
				.ThrowsAsync(new ApiException("Invalid Credentials"));

			// Act & Assert
			Assert.ThrowsAsync<ApiException>(async () => await _controller.AuthenticateAsync(authRequest));
		}
	}
}