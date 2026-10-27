using FluentValidation;

namespace DocManager.Application.Notes;

public sealed class CreateNoteRequestValidator : AbstractValidator<CreateNoteRequest>
{
    public const int NoteTextMaxLength = 1000;
    private const decimal MinPercent = 0m;
    private const decimal MaxPercent = 100m;

    public CreateNoteRequestValidator()
    {
        RuleFor(r => r.NoteText)
            .NotEmpty().WithMessage("Note text is required.")
            .MaximumLength(NoteTextMaxLength).WithMessage($"Note text must be {NoteTextMaxLength} characters or fewer.");

        RuleFor(r => r.PositionTop)
            .InclusiveBetween(MinPercent, MaxPercent).WithMessage("Top position must be between 0 and 100.");

        RuleFor(r => r.PositionLeft)
            .InclusiveBetween(MinPercent, MaxPercent).WithMessage("Left position must be between 0 and 100.");
    }
}
