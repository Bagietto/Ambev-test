using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetUserHandler"/> class.
/// </summary>
public class GetUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly GetUserHandler _handler;

    public GetUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetUserHandler(_userRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid user ID When retrieving user Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        // Given
        var userId = Guid.NewGuid();
        var command = new GetUserCommand(userId);
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com"
        };
        var result = new GetUserResult
        {
            Id = userId,
            Name = "testuser",
            Email = "test@example.com"
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<GetUserResult>(user).Returns(result);

        // When
        var handleResult = await _handler.Handle(command, CancellationToken.None);

        // Then
        handleResult.Should().NotBeNull();
        handleResult.Id.Should().Be(userId);
        handleResult.Name.Should().Be("testuser");
        await _userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given empty user ID When retrieving user Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Given
        var command = new GetUserCommand(Guid.Empty); // Empty ID will fail validation

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given non-existing user ID When retrieving user Then throws key not found exception")]
    public async Task Handle_NonExistingUser_ThrowsKeyNotFoundException()
    {
        // Given
        var userId = Guid.NewGuid();
        var command = new GetUserCommand(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
