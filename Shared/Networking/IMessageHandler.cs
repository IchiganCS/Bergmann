namespace Bergmann.Shared.Networking;

public interface IMessageHandler<T> where T : IMessage {
    public void HandleMessage(T message);

    public void HandleAnyMessage(IMessage message) {
        if (message is T t)
            HandleMessage(t);
    }
}