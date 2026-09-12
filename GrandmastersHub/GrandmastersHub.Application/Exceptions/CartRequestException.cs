namespace GrandmastersHub.Application.Exceptions;

public enum CartError { InvalidRequest, NotFound, Conflict, Unauthenticated }

public sealed class CartRequestException(CartError error, string message) : Exception(message)
{
    public CartError Error { get; } = error;
}
