using FluentResults;

namespace NVs.Budget.Controllers.Web.Exceptions;

public abstract class HttpException : Exception
{
    public int StatusCode { get; }
    public IEnumerable<IError> Errors { get; }

    protected HttpException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
        Errors = new List<IError> { new Error(message) };
    }

    protected HttpException(int statusCode, IEnumerable<IError> errors) : base(string.Join(", ", errors.Select(e => e.Message)))
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}

public class BadRequestException : HttpException
{
    public BadRequestException(string message) : base(400, message)
    {
    }

    public BadRequestException(IEnumerable<IError> errors) : base(400, errors)
    {
    }
}

public class NotFoundException : HttpException
{
    public NotFoundException(string message) : base(404, message)
    {
    }

    public NotFoundException(IEnumerable<IError> errors) : base(404, errors)
    {
    }
}

public class UnauthorizedException : HttpException
{
    public UnauthorizedException(string message) : base(401, message)
    {
    }

    public UnauthorizedException(IEnumerable<IError> errors) : base(401, errors)
    {
    }
}

public class ForbiddenException : HttpException
{
    public ForbiddenException(string message) : base(403, message)
    {
    }

    public ForbiddenException(IEnumerable<IError> errors) : base(403, errors)
    {
    }
}

public class ConflictException : HttpException
{
    public ConflictException(string message) : base(409, message)
    {
    }

    public ConflictException(IEnumerable<IError> errors) : base(409, errors)
    {
    }
}

