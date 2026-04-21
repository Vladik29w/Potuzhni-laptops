using MediatR;
using System.Threading.Channels;

namespace LaptopServer.Infrastructure.Notification
{
    public record OrderNotification(Guid orderId) : INotification;

    public class OrderProcessingChannel
    {
        private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();
        public ChannelReader<Guid> Reader => _channel.Reader;
        public ChannelWriter<Guid> Writer => _channel.Writer;
    }
    public class OrderHandler(OrderProcessingChannel channel, ILogger<OrderHandler> logger) : INotificationHandler<OrderNotification>
    {
        public async Task Handle(OrderNotification order, CancellationToken ct = default)
        {
            logger.LogInformation($"{order.orderId} is confirmed, sending to channel");

            await channel.Writer.WriteAsync(order.orderId, ct);
        }
    }
}