namespace BuildingBlocks.Application.Exceptions;

public class BadRequestException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{  }
