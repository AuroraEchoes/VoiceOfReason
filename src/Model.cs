namespace VoiceOfReason
{
    // TODO: In the longer term, I’d like to unify all commands into a single command specification
    // But that would require a lot more work, thinking, and time than I have to dedicate right now…
    public struct Config
    {
        public ConfirmEvent ConfirmEvent;
        public AvailabilityPoll AvailabilityPoll;
        public AnnounceEvent AnnounceEvent;
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

    public struct AnnounceEvent
    {
        public List<Field> Fields;
        public List<string> Reactions;
        public string Footer;
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

    public class Field
    {
        public string id = null!;
        public List<string> IncludeTypes = null!;
        public string Emoji = null!;
        public string Emote = null!;
        public string Label = null!;
        public bool Bold = true;
        public List<Field> Subfields = null!;

        public string ToString(Field field, Dictionary<string, string> values, int depth = 0)
        {
            string value = values.ContainsKey(field.id) ? values[field.id] : "";
            string icon = field.Emote is not null ? field.Emote : field.Emoji;
            string nameSurround = field.Bold ? "**" : "";
            // int depth = FieldDepth(field);
            string indent = depth > 0 ? "> " + string.Concat(Enumerable.Repeat(" . ", depth - 1)) : "";
            string buf = $"{indent}{icon} {nameSurround}{field.Label}{nameSurround}: {value}";
            if (field.Subfields is not null && field.Subfields.Count > 0)
            {
                buf += "\n" + string.Join("\n", field.Subfields.Select(f => ToString(f, values, depth + 1)));
            }
            return buf;
        }

        private int FieldDepth(Field field)
        {
            return RecurseFieldDepth(field, Configuration.Config.ConfirmEvent.Fields);
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

    public struct Reaction
    {
        public string Emoji;
        public string Emote;
        public List<string> IncludeTypes;
    }

}
