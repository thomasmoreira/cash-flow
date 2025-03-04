using CashFlow.Application.Contracts;
using CashFlow.Consolidating.Messaging;
using CashFlow.Domain.Events;
using FluentAssertions;
using MassTransit;
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

        [Fact]
        public async Task TransactionConsumer_Should_Call_Consolidate_For_Multiple_Events()
        {

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

                var event1 = new TransactionCreatedEvent
                {
                    TransactionId = Guid.NewGuid(),

                };
                var event2 = new TransactionCreatedEvent
                {
                    TransactionId = Guid.NewGuid(),

                };


                await harness.InputQueueSendEndpoint.Send(event1);
                await harness.InputQueueSendEndpoint.Send(event2);


                bool consumed = await harness.Consumed.Any<TransactionCreatedEvent>();
                consumed.Should().BeTrue("o consumer deve consumir pelo menos um evento");


                consolidationServiceMock.Verify(s => s.Consolidate(event1.TransactionId), Times.Once,
                    "o método Consolidate deve ser chamado uma vez para event1");
                consolidationServiceMock.Verify(s => s.Consolidate(event2.TransactionId), Times.Once,
                    "o método Consolidate deve ser chamado uma vez para event2");
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task TransactionConsumer_Should_Handle_Exception_From_ConsolidationService()
        {

            var consolidationServiceMock = new Mock<IConsolidationService>();

            consolidationServiceMock
                .Setup(s => s.Consolidate(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Simulated consolidation failure"));

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


                var consumed = await harness.Consumed.Any<TransactionCreatedEvent>();
                consumed.Should().BeTrue("o consumer deve consumir o evento, mesmo que falhe");


                consolidationServiceMock.Verify(s => s.Consolidate(testEvent.TransactionId), Times.Once,
                    "o método Consolidate deve ser chamado mesmo se lançar exceção");


                var publishedFault = await harness.Published.Any<Fault<TransactionCreatedEvent>>();
                publishedFault.Should().BeTrue("uma mensagem fault deve ser publicada quando ocorrer exceção no consumer");
            }
            finally
            {
                await harness.Stop();
            }
        }

    }
}
