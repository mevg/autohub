namespace AutoHub.Application.Common.Exceptions;

public sealed class IdempotencyConflictException(string message) : Exception(message);
