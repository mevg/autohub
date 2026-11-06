namespace AutoHub.Application.Common.Exceptions;

public sealed class DuplicateIdempotencyKeyException(string key) : Exception($"Idempotency key '{key}' ya fue usada.");
