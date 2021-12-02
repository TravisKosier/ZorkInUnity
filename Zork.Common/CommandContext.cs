namespace Zork.Common
{
    public class CommandContext
    {
        public Game Game { get; }
        public string CommandString { get; }
        public string Verb { get; set; }
        public string Subject { get; set; }
        public string SubjectTwo { get; set; }

        public CommandContext(Game game, string commandString, string verb = null, string subject = null, string subjectTwo = null)
        {
            Game = game;
            CommandString = commandString;
            Verb = verb;
            Subject = subject;
            SubjectTwo = subjectTwo;
        }
    }
}
