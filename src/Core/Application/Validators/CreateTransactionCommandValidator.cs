using CashFlow.Application.Commands;
using CashFlow.Domain.Enums;
using FluentValidation;

namespace CashFlow.Application.Validators;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Type)
            .Must(value => Enum.IsDefined(typeof(TransactionType), value))
            .WithMessage("O tipo de transação deve ser Debito ou Credito.");

        RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("O valor deve ser maior que zero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(100).WithMessage("A descrição deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("A data do lançamento é obrigatória.");
    }
}
