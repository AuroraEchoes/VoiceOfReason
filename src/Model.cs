namespace VoiceOfReason
{

    public struct Config
    {
        public ConfirmEvent ConfirmEvent;
    }

    public struct ConfirmEvent
    {
        public List<Type> Types;
        public List<Field> Fields;
        public List<Reaction> Reactions;
        public string Footer;
    }

    public struct Type
    {
        public string id;
        public string Name;
        public string Description;

        public ISlashCommand.Subcommand ToSubcommand()
        {
            return new ISlashCommand.Subcommand { ID = this.id, Description = this.Description };
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
