using Discord.WebSocket;

namespace VoiceOfReason
{
    public interface ISlashCommand
    {
        public string Name { get; }
        public string Description { get; }
        public Task Run(SocketSlashCommand context);
    }
}
