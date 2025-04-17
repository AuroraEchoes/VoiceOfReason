using Discord;
using Discord.WebSocket;

namespace VoiceOfReason
{
    public class DiscordBot
    {
        private DiscordSocketClient m_Client = null!;

        public DiscordBot()
        {
            DiscordSocketConfig config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            };

            m_Client = new DiscordSocketClient(config);

            m_Client.Log += OnLogEvent;
            m_Client.Ready += OnReadyEvent;
        }

        ~DiscordBot()
        {
            m_Client.Log -= OnLogEvent;
            m_Client.Ready -= OnReadyEvent;
            m_Client.StopAsync();
        }

        public async Task Login(string token)
        {
            await m_Client.LoginAsync(TokenType.Bot, token);
            await m_Client.StartAsync();
        }

        private static Task OnReadyEvent()
        {
            Console.WriteLine("Connected to Discord");
            return Task.CompletedTask;
        }

        private static Task OnLogEvent(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

    }
}
