using Discord;
using Discord.WebSocket;

namespace VoiceOfReason
{
    public interface ISlashCommand
    {
        public string Name { get; }
        public string Description { get; }
        public List<Parameter> Parameters { get; }

        public Task Run(SocketSlashCommand context);

        public struct Parameter
        {
            public ApplicationCommandOptionType Type;
            public string ID;
            public string Description;
        }
    }
}
