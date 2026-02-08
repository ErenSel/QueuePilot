namespace QueuePilot.Application.Common.Exceptions;

public class MessagingUnavailableException : Exception
{
    public MessagingUnavailableException(string message) : base(message)
    {
    }
}
