using CashFlow.Application.Commands;
using FluentValidation;

namespace CashFlow.Application.Validators;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("O valor deve ser maior que zero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(100).WithMessage("A descrição deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("A data do lançamento é obrigatória.");

        //RuleFor(x => x.Type)
        //    .Must(tipo => string.Equals(tipo, "debito", StringComparison.OrdinalIgnoreCase)
        //                || string.Equals(tipo, "credito", StringComparison.OrdinalIgnoreCase))
        //    .WithMessage("O tipo de lançamento deve ser 'debito' ou 'credito'.");
    }
}
