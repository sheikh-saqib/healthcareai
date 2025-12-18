namespace HealthCareAI.Application.Exceptions;

public class ConsultationNotFoundException : Exception
{
    public ConsultationNotFoundException(string id)
        : base($"Consultation with ID {id} was not found.")
    {
    }
}

public class InvalidConsultationStateException : Exception
{
    public InvalidConsultationStateException(string message)
        : base(message)
    {
    }
}

public class ConsultationValidationException : Exception
{
    public ConsultationValidationException(string message)
        : base(message)
    {
    }
} 