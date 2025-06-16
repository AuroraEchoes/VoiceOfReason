using Discord;
using Discord.Rest;
using Discord.WebSocket;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VoiceOfReason
{
    public class ConfirmEventSlashEvent : ISlashCommand
    {
        public string Name { get; } = "confirm-event";
        public string Description { get; } = "Confirm an event.";
        public List<ISlashCommand.Subcommand> Subcommands => m_Config.Types.Select(t => t.ToSubcommand()).ToList();

        private ConfirmEvent m_Config;
        private InteractionManager m_InteractionManager;

        public ConfirmEventSlashEvent(InteractionManager interactionManager)
        {
            string configFile = File.ReadAllText("config.yaml");
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            m_Config = deserializer.Deserialize<Config>(configFile).ConfirmEvent;
            m_InteractionManager = interactionManager;
        }

        public async Task Run(SocketSlashCommand context)
        {
            string type = context.Data.Options.First().Name;
            string name = m_Config.Types.Where(t => t.id.Equals(type)).First().Name;
            List<Field> fields = m_Config.Fields
                .Where(f => f.IncludeTypes is null || f.IncludeTypes.Contains(type)).ToList();
            MultiPageModal modal = new MultiPageModal($"{name} Event Confirmation", fields, m_InteractionManager);
            Dictionary<string, string> values = await modal.BeginSendingMultiPageModal(context);
            RestUserMessage message = await SendMessage(values, context.ChannelId, name, fields);
            List<Reaction> reactions = m_Config.Reactions
                .Where(f => f.IncludeTypes is null || f.IncludeTypes.Contains(type)).ToList();
            await AddReactions(message, reactions);
        }

        public async Task<RestUserMessage> SendMessage(Dictionary<string, string> values, ulong? channelID, string name, List<Field> fields)
        {
            if (channelID is null) return null!;
            SocketTextChannel channel = await m_InteractionManager.GetChannel((ulong)channelID!);
            return await channel.SendMessageAsync("[ @everyone ]", embed: BuildConfirmEventEmbed(values, name, fields, m_Config.Footer));
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
                .WithDescription(string.Join("\n\n", fields.Select(f => FieldToString(f, values)).Append(footer)))
                .Build();
        }

        private string FieldToString(Field field, Dictionary<string, string> values, int depth = 0)
        {
            string value = values.ContainsKey(field.id) ? values[field.id] : "";
            string icon = field.Emote is not null ? field.Emote : field.Emoji;
            string nameSurround = field.Bold ? "**" : "";
            // int depth = FieldDepth(field);
            string indent = depth > 0 ? "> " + string.Concat(Enumerable.Repeat(" . ", depth - 1)) : "";
            string buf = $"{indent}{icon} {nameSurround}{field.Label}{nameSurround}: {value}";
            if (field.Subfields is not null && field.Subfields.Count > 0)
            {
                buf += "\n" + string.Join("\n", field.Subfields.Select(f => FieldToString(f, values, depth + 1)));
            }
            return buf;
        }

        private int FieldDepth(Field field)
        {
            return RecurseFieldDepth(field, m_Config.Fields);
        }

        private int RecurseFieldDepth(Field field, List<Field> fields, int currDepth = 0)
        {
            Console.WriteLine("Recursing");
            foreach (Field f in fields)
            {
                if (f.id == field.id)
                    return currDepth;
                else if (f.Subfields is not null)
                    RecurseFieldDepth(field, f.Subfields, currDepth + 1);
            }
            return currDepth;
        }
    }
}
