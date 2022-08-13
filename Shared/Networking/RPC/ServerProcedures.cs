public class ServerProcedures {


    public delegate void ChatMessageSentDelegate(string username, string message);
    public event ChatMessageSentDelegate OnChatMessageSent = delegate { };
}