namespace VoiceOfReason
{
    // TODO: In the longer term, I’d like to unify all commands into a single command specification
    // But that would require a lot more work, thinking, and time than I have to dedicate right now…
    public struct Config
    {
        public ConfirmEvent ConfirmEvent;
        public AvailabilityPoll AvailabilityPoll;
    }

    public struct ConfirmEvent
    {
        public List<Type> Types;
        public List<Field> Fields;
        public List<Reaction> Reactions;
        public string Footer;
    }

    public struct AvailabilityPoll
    {
        public string Header;
    }

    public struct Type
    {
        public string id;
        public string Name;
        public string Description;

        public ISlashCommand.Parameter ToSubcommand()
        {
            return new ISlashCommand.Parameter
            {
                Type = Discord.ApplicationCommandOptionType.SubCommand,
                ID = this.id,
                Description = this.Description
            };
        }
    }

    public struct Field
    {
        public string id;
        public List<string> IncludeTypes;
        public string Emoji;
        public string Emote;
        public string Label;
        public bool Bold;
        public List<Field> Subfields;
    }

    public struct Reaction
    {
        public string Emoji;
        public string Emote;
        public List<string> IncludeTypes;
    }

}
