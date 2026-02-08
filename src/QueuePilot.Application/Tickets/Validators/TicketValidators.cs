using FluentValidation;
using QueuePilot.Application.Tickets.Commands;

namespace QueuePilot.Application.Tickets.Validators;

public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");
    }
}

public class AddTicketCommentCommandValidator : AbstractValidator<AddTicketCommentCommand>
{
    public AddTicketCommentCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty();
        
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Comment text cannot be empty.");
    }
}
