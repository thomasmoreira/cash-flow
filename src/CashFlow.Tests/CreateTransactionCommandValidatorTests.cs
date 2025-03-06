using CashFlow.Application.Commands;
using CashFlow.Application.Validators;
using CashFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace CashFlow.Tests
{
    public class CreateTransactionCommandValidatorTests
    {
        private readonly CreateTransactionCommandValidator _validator;

        public CreateTransactionCommandValidatorTests()
        {
            _validator = new CreateTransactionCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Amount_Is_Zero()
        {
            // Arrange
            var command = new CreateTransactionCommand
            {
                Date = DateTime.Now,
                Type = (int)TransactionType.Credit,
                Amount = 0,
                Description = "Teste"
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                  .WithErrorMessage("O valor deve ser maior que zero.");
        }

        [Fact]
        public void Should_Have_Error_When_Type_Is_Invalid()
        {
            // Arrange
            var command = new CreateTransactionCommand
            {
                Date = DateTime.Now,
                Type = 3,
                Amount = 100,
                Description = "Teste"
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Type)
                  .WithErrorMessage("O tipo de transação deve ser Debito ou Credito.");
        }

        [Fact]
        public void Should_Have_Error_When_Description_Is_Empty()
        {
            // Arrange
            var command = new CreateTransactionCommand
            {
                Date = DateTime.Now,
                Type = (int)TransactionType.Debit,
                Amount = 50,
                Description = ""
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("A descrição é obrigatória.");
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Command()
        {
            // Arrange
            var command = new CreateTransactionCommand
            {
                Date = DateTime.Now,
                Type = (int) TransactionType.Debit,
                Amount = 100,
                Description = "Transação válida"
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
