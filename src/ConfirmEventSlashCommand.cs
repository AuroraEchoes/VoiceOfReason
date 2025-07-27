using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace VoiceOfReason
{
    public class ConfirmEventSlashEvent : ISlashCommand
    {
        public string Name { get; } = "confirm-event";
        public string Description { get; } = "Confirm an event.";
        public List<ISlashCommand.Parameter> Parameters => Configuration.Config.ConfirmEvent.Types.Select(t => t.ToSubcommand()).ToList();

        private InteractionManager m_InteractionManager;

        public ConfirmEventSlashEvent(InteractionManager interactionManager)
        {
            m_InteractionManager = interactionManager;
        }

        public async Task Run(SocketSlashCommand context)
        {
            string type = context.Data.Options.First().Name;
            string name = Configuration.Config.ConfirmEvent.Types.Where(t => t.id.Equals(type)).First().Name;
            List<Field> fields = Configuration.Config.ConfirmEvent.Fields
                .Where(f => f.IncludeTypes is null || f.IncludeTypes.Contains(type)).ToList();
            MultiPageModal modal = new MultiPageModal($"{name} Event Confirmation", fields, m_InteractionManager);
            Dictionary<string, string> values = await modal.BeginSendingMultiPageModal(context);
            RestUserMessage message = await SendMessage(values, context.ChannelId, name, fields);
            List<Reaction> reactions = Configuration.Config.ConfirmEvent.Reactions
                .Where(f => f.IncludeTypes is null || f.IncludeTypes.Contains(type)).ToList();
            await AddReactions(message, reactions);
        }

        public async Task<RestUserMessage> SendMessage(Dictionary<string, string> values, ulong? channelID, string name, List<Field> fields)
        {
            if (channelID is null) return null!;
            SocketTextChannel channel = await m_InteractionManager.GetChannel((ulong)channelID!);
            return await channel.SendMessageAsync("[ @everyone ]", embed: BuildConfirmEventEmbed(values, name, fields, Configuration.Config.ConfirmEvent.Footer));
        }

        public async Task AddReactions(RestUserMessage message, List<Reaction> reactions)
        {
            foreach (Reaction r in reactions)
            {
                await message.AddReactionAsync(r.Emote is not null ? Emote.Parse(r.Emote) : Emoji.Parse(r.Emoji));
            }
        }

        private Embed BuildConfirmEventEmbed(Dictionary<string, string> values, string name, List<Field> fields, string footer)
        {
            return new EmbedBuilder()
                .WithTitle($"{name} Event Confirmation")
                .WithDescription(string.Join("\n\n", fields.Select(f => f.ToString(f, values)).Append(footer)))
                .Build();
        }

    }
}
