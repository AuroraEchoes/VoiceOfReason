using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace VoiceOfReason
{
    public class AnnounceEventSlashCommand : ISlashCommand
    {
        public string Name { get; } = "announce-event";

        public string Description { get; } = "Announce an event";

        public List<ISlashCommand.Parameter> Parameters { get; } = new List<ISlashCommand.Parameter>();

        private InteractionManager m_InteractionManager;

        public AnnounceEventSlashCommand(InteractionManager interactionManager)
        {
            m_InteractionManager = interactionManager;
            // TODO: At some point this would ideally be defined in configuration
            // (I just haven’t figured out a sensible way to do that yet)
            Parameters.Add(new ISlashCommand.Parameter
            {
                Type = ApplicationCommandOptionType.String,
                ID = "start-date",
                Description = "Start Date (YYYY-MM-DD)"
            });
        }

        public async Task Run(SocketSlashCommand context)
        {
            string dateStr = (string)context.Data.Options.First().Value;

            DateTime date;
            if (DateTime.TryParse(dateStr, out date))
            {
                List<Field> fields = Configuration.Config.AnnounceEvent.Fields;
                MultiPageModal modal = new MultiPageModal($"Event Announcement", fields, m_InteractionManager);
                Dictionary<string, string> values = await modal.BeginSendingMultiPageModal(context);
                RestUserMessage message = await SendMessage(context.ChannelId, date, fields, values);
                await message.AddReactionsAsync(Configuration.Config.AnnounceEvent.Reactions.Select(s => Emoji.Parse(s)));
            }
            else
            {
                await context.RespondAsync(embed: BuildInvalidDateFormatEmbed(dateStr), ephemeral: true);
            }
        }

        public async Task<RestUserMessage> SendMessage(ulong? channelID, DateTime startDate, List<Field> fields, Dictionary<string, string> values)
        {
            if (channelID is null) return null!;
            SocketTextChannel channel = await m_InteractionManager.GetChannel((ulong)channelID!);
            return await channel.SendMessageAsync("[ @everyone ]", embed: BuildAnnounceEventEmbed(startDate, fields, values));
        }

        private Embed BuildInvalidDateFormatEmbed(string dateStr)
        {
            return new EmbedBuilder()
                .WithTitle($"Invalid Date")
                .WithDescription($"{dateStr} is not a valid date. Dates must be in the form YYYY-MM-DD.")
                .Build();
        }

        private Embed BuildAnnounceEventEmbed(DateTime startDate, List<Field> fields, Dictionary<string, string> values)
        {
            string fieldsStr = string.Join("\n\n", fields.Select(f => f.ToString(f, values)));
            return new EmbedBuilder()
                .WithTitle($"Event Announcement")
                .WithDescription($"{fieldsStr}\n\n{DayTimeStrings(startDate)}\n\n{Configuration.Config.AnnounceEvent.Footer}")
                .Build();
        }

        private string DayTimeStrings(DateTime date)
        {
            TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");
            return string.Join("\n", Enumerable.Range(0, 5).Select(n =>
            {
                DateTime time = new DateTime(DateOnly.FromDateTime(date), new TimeOnly(n + 18, 0));
                time = TimeZoneInfo.ConvertTimeToUtc(time, timezone);
                long timestamp = ((DateTimeOffset)time).ToUnixTimeSeconds();
                return $"{Configuration.Config.AnnounceEvent.Reactions[n]} →  {n + 6} PM (<t:{timestamp}:t>)";
            }));
        }
    }
}
