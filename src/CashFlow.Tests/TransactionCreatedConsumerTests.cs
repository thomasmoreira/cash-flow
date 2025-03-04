using CashFlow.Application.Contracts;
using CashFlow.Consolidating.Messaging;
using CashFlow.Domain.Events;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CashFlow.Tests
{
    public class TransactionCreatedConsumerTests
    {
        [Fact]
        public async Task TransactionConsumer_Should_Call_Consolidate()
        {
            // Arrange
            var consolidationServiceMock = new Mock<IConsolidationService>();
            consolidationServiceMock
                .Setup(s => s.Consolidate(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            var harness = new InMemoryTestHarness();

            var consumerHarness = harness.Consumer(() =>
                new TransactionConsumer(NullLogger<TransactionConsumer>.Instance, consolidationServiceMock.Object));

            await harness.Start();
            try
            {
                var testEvent = new TransactionCreatedEvent
                {
                    TransactionId = Guid.NewGuid(),
                };

                await harness.InputQueueSendEndpoint.Send(testEvent);

                bool consumed = await harness.Consumed.Any<TransactionCreatedEvent>();
                consumed.Should().BeTrue("o consumer deve consumir o evento");

                consolidationServiceMock.Verify(s => s.Consolidate(testEvent.TransactionId), Times.Once,
                    "o método Consolidate deve ser chamado uma vez com o TransactionId do evento");
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}
