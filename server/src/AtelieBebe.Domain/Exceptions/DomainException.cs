namespace AtelieBebe.Domain.Exceptions;

/// <summary>Raised when a domain invariant is violated.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
