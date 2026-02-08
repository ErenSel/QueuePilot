using QueuePilot.Domain.Enums;

namespace QueuePilot.Application.Common.Models;

public record TicketQueryParameters(
    int Page,
    int PageSize,
    TicketStatus? Status,
    string? SearchTerm,
    DateTime? FromDate,
    DateTime? ToDate);
