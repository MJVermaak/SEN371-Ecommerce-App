namespace GrandmastersHub.Application.Exceptions;

public enum OrderFailure { InvalidRequest, Unauthenticated, NotFound, Conflict }

public sealed class OrderRequestException(OrderFailure failure, string message) : Exception(message)
{
    public OrderFailure Failure { get; } = failure;
}
