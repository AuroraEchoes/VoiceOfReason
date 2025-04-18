using Discord.WebSocket;
using Discord;
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

        public ConfirmEventSlashEvent()
        {
            string configFile = File.ReadAllText("config.yaml");
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            m_Config = deserializer.Deserialize<Config>(configFile).ConfirmEvent;
        }

        public async Task Run(SocketSlashCommand context)
        {
            string typeName = context.Data.Options.First().Name;
            Type type = m_Config.Types.Where(t => t.id.Equals(typeName)).First();
            ModalBuilder builder = new ModalBuilder()
                .WithTitle($"{type.Name} Event Announcement")
                .WithCustomId($"{Name}-{type.id}-modal");
            foreach (Field field in m_Config.Fields)
            {
                if (field.IncludeTypes is null || field.IncludeTypes.Contains(type.id))
                    await AddFieldToModal(field, builder);
            }
            Modal modal = builder.Build();
            await context.RespondWithModalAsync(modal);
        }

        private async Task AddFieldToModal(Field field, ModalBuilder builder)
        {
            await AddFieldToModalRecursive(field, builder, 0, "");
        }

        private async Task AddFieldToModalRecursive(Field field, ModalBuilder builder, int depth, string prefix)
        {
            builder.AddTextInput(new TextInputBuilder()
                .WithCustomId(field.id)
                .WithLabel($"{prefix}{field.Label}")
                .WithStyle(TextInputStyle.Short)
            );
            if (field.Subfields is null)
                return;
            foreach (Field subField in field.Subfields)
            {
                await AddFieldToModalRecursive(subField, builder, depth + 1, $"{prefix} → ");
            }
        }

        // TODO: This shouldn’t remain here
        private struct Config
        {
            public ConfirmEvent ConfirmEvent;
        }

        private struct ConfirmEvent
        {
            public List<Type> Types;
            public List<Field> Fields;
        }

        private struct Type
        {
            public string id;
            public string Name;
            public string Description;

            public ISlashCommand.Subcommand ToSubcommand()
            {
                return new ISlashCommand.Subcommand { ID = this.id, Description = this.Description };
            }
        }

        private struct Field
        {
            public string id;
            public List<string> IncludeTypes;
            public string Emoji;
            public string Emote;
            public string Label;
            public bool Bold;
            public List<Field> Subfields;
        }
    }
}
