using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace VoiceOfReason
{
    public class PollAvailabilitySlashCommand : ISlashCommand
    {
        public string Name { get; } = "poll-availability";

        public string Description { get; } = "Poll availability for a week";

        public List<ISlashCommand.Parameter> Parameters { get; } = new List<ISlashCommand.Parameter>();

        private InteractionManager m_InteractionManager;

        // TODO: No.
        private static readonly Dictionary<DayOfWeek, Emote> s_WeekDayEmotes = new Dictionary<DayOfWeek, Emote>
        {
            { DayOfWeek.Sunday, Emote.Parse("<:weekday_sunday:1384377139524009996>") },
            { DayOfWeek.Monday, Emote.Parse("<:weekday_monday:1384377088357826580>") },
            { DayOfWeek.Tuesday, Emote.Parse("<:weekday_tuesday:1384377175750217738>") },
            { DayOfWeek.Wednesday, Emote.Parse("<:weekday_wednesday:1384377193756364831>") },
            { DayOfWeek.Thursday, Emote.Parse("<:weekday_thursday:1384377156184047697>") },
            { DayOfWeek.Friday, Emote.Parse("<:weekday_friday:1384377061296050206>") },
            { DayOfWeek.Saturday, Emote.Parse("<:weekday_friday:1384377061296050206>") }

        };

        public PollAvailabilitySlashCommand(InteractionManager interactionManager)
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
                await context.RespondAsync("Polling availability", ephemeral: true);
                RestUserMessage message = await SendMessage(context.ChannelId, date);
                await message.AddReactionsAsync(WeekDateEmotes(date));
            }
            else
            {
                await context.RespondAsync(embed: BuildInvalidDateFormatEmbed(dateStr), ephemeral: true);
            }
        }

        public async Task<RestUserMessage> SendMessage(ulong? channelID, DateTime startDate)
        {
            if (channelID is null) return null!;
            SocketTextChannel channel = await m_InteractionManager.GetChannel((ulong)channelID!);
            return await channel.SendMessageAsync("[ @everyone ]", embed: BuildAvailabilityPollEmbed(startDate));
        }

        private Embed BuildInvalidDateFormatEmbed(string dateStr)
        {
            return new EmbedBuilder()
                .WithTitle($"Invalid Date")
                .WithDescription($"{dateStr} is not a valid date. Dates must be in the form YYYY-MM-DD.")
                .Build();
        }

        private Embed BuildAvailabilityPollEmbed(DateTime startDate)
        {
            return new EmbedBuilder()
                .WithTitle($"Availability Poll")
                .WithDescription($"{Configuration.Config.AvailabilityPoll.Header}\n{WeekDateStrings(startDate)}")
                .Build();
        }

        private string WeekDateStrings(DateTime startDate)
        {
            return string.Join("\n", Enumerable.Range(0, 7).Select(n =>
            {
                DateTime day = startDate.AddDays(n);
                DayOfWeek weekday = day.DayOfWeek;
                long timestamp = ((DateTimeOffset)day).ToUnixTimeSeconds();
                return $"{s_WeekDayEmotes[weekday].ToString()} →  {weekday.ToString()} (<t:{timestamp}:D>)";
            }));
        }

        private IEnumerable<Emote> WeekDateEmotes(DateTime startDate)
        {
            return Enumerable.Range(0, 7).Select(n => s_WeekDayEmotes[startDate.AddDays(n).DayOfWeek]);
        }
    }
}
