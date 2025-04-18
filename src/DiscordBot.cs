using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace VoiceOfReason
{
    public class DiscordBot
    {
        private DiscordSocketClient m_Client = null!;
        private Dictionary<string, ISlashCommand> m_SlashCommands = new Dictionary<string, ISlashCommand>();

        public DiscordBot()
        {
            DiscordSocketConfig config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            };

            m_Client = new DiscordSocketClient(config);

            m_Client.Log += OnLogEvent;
            m_Client.Ready += OnReadyEvent;
            m_Client.SlashCommandExecuted += OnSlashCommandExecutedEvent;
        }

        ~DiscordBot()
        {
            m_Client.Log -= OnLogEvent;
            m_Client.Ready -= OnReadyEvent;
            m_Client.SlashCommandExecuted -= OnSlashCommandExecutedEvent;
            m_Client.StopAsync();
        }

        public async Task Login(string token)
        {
            await m_Client.LoginAsync(TokenType.Bot, token);
            await m_Client.StartAsync();
        }

        private async Task RegisterCommand(ISlashCommand command)
        {
            SlashCommandBuilder builder = new SlashCommandBuilder()
                .WithName(command.Name)
                .WithDescription(command.Description);
            foreach (ISlashCommand.Subcommand subcmd in command.Subcommands)
            {
                builder.AddOption(new SlashCommandOptionBuilder()
                    .WithName(subcmd.ID)
                    .WithDescription(subcmd.Description)
                    .WithType(ApplicationCommandOptionType.SubCommand)
                );
            }
            SlashCommandProperties cmd = builder.Build();
            try
            {
                await m_Client.CreateGlobalApplicationCommandAsync(cmd);
            }
            catch (HttpException exception)
            {
                Console.WriteLine(exception.ToString());
            }
            m_SlashCommands[command.Name] = command;
            Console.WriteLine($"Registered command: {command.Name}");
        }

        private async Task OnReadyEvent()
        {
            Console.WriteLine("Connected to Discord");
            await RegisterCommand(new ConfirmEventSlashEvent());
        }

        private Task OnLogEvent(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task OnSlashCommandExecutedEvent(SocketSlashCommand command)
        {
            await m_SlashCommands[command.Data.Name].Run(command);
        }
    }
}
