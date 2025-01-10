namespace ChatBot.Domain.Chats;

public sealed class UserHistory
{
    public Guid Id { get; set; }
    public string Message { get; set; }
}