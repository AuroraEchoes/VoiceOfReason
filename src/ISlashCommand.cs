using Discord.WebSocket;

namespace VoiceOfReason
{
    public interface ISlashCommand
    {
        public string Name { get; }
        public string Description { get; }
        public List<Subcommand> Subcommands { get; }

        public Task Run(SocketSlashCommand context);

        public struct Subcommand
        {
            public string ID;
            public string Description;
        }
    }
}
