namespace VoiceOfReason
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            DiscordBot bot = new DiscordBot();
            await bot.Login(await ReadBotToken());
            await Task.Delay(Timeout.Infinite);
        }

        public static async Task<string> ReadBotToken()
        {
            string secretsFile = await File.ReadAllTextAsync("secrets.txt");
            return secretsFile.Trim();
        }
    }
}
