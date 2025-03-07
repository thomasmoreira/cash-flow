using CashFlow.Application.Commands;
using CashFlow.Application.Handlers;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;
using CashFlow.Domain.Events;
using CashFlow.Domain.Repositories;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace CashFlow.Tests
{
    public class CreateTransactionCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Create_Transaction_And_Publish_Event()
        {
            // Arrange
            var repositoryMock = new Mock<ITransactionRepository>();
            repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);

            var busMock = new Mock<IBus>();
            busMock
                .Setup(b => b.Publish(It.IsAny<TransactionCreatedEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<CreateTransactionCommandHandler>>();

            var handler = new CreateTransactionCommandHandler(
                repositoryMock.Object,
                busMock.Object,
                loggerMock.Object);

            var now = DateTime.Now;
            var command = new CreateTransactionCommand
            {
                Date = now,
                Type = (int)TransactionType.Credit,
                Amount = 100.0m,
                Description = "Teste de transação"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            
            repositoryMock.Verify(r => r.AddAsync(It.Is<Transaction>(t =>
                t.Amount == command.Amount &&
                t.Description == command.Description &&
                t.Type == (TransactionType)command.Type &&
                t.Date.Date == now.Date  // Considerando apenas a data
            )), Times.Once);

            busMock.Verify(b => b.Publish(It.Is<TransactionCreatedEvent>(e =>
                e.TransactionId == result
            ), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
