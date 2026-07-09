namespace Application.Common.Exceptions;

public class BadRequestException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{  }
