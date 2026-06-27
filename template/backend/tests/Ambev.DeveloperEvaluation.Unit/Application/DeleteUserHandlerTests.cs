using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="DeleteUserHandler"/> class.
/// </summary>
public class DeleteUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new DeleteUserHandler(_userRepository);
    }

    [Fact(DisplayName = "Given valid user ID When deleting user Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        // Given
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        _userRepository.DeleteAsync(userId, Arg.Any<CancellationToken>()).Returns(true);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        await _userRepository.Received(1).DeleteAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given empty user ID When deleting user Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Given
        var command = new DeleteUserCommand(Guid.Empty); // Empty ID will fail validation

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given non-existing user ID When deleting user Then throws key not found exception")]
    public async Task Handle_NonExistingUser_ThrowsKeyNotFoundException()
    {
        // Given
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        _userRepository.DeleteAsync(userId, Arg.Any<CancellationToken>()).Returns(false);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
