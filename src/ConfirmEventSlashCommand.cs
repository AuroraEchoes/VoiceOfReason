using Discord.WebSocket;

namespace VoiceOfReason
{
    public class ConfirmEventSlashEvent : ISlashCommand
    {
        public string Name { get; } = "confirm-event";

        public string Description { get; } = "Confirm an event.";

        public async Task Run(SocketSlashCommand context)
        {
            await context.RespondAsync("Creating a new event.", ephemeral: true);
        }
    }
}
