using System;
using System.Collections.Generic;
using System.Linq;

namespace Zork.Common
{
    public class Command
    {
        public string Name { get; set; }
        
        public string[] Verbs { get; set; }

        public string Description { get; set; }

        public string VerbAliasList { get; set; }

        public Action<CommandContext> Action { get; set; }

        public Command(string name, IEnumerable<string> verbs, Action<CommandContext> action, string description = null)
        {
            Assert.IsNotNull(name);
            Assert.IsNotNull(verbs);
            Assert.IsNotNull(action);

            Name = name;
            Verbs = verbs.ToArray();
            Action = action;
            Description = description;
            VerbAliasList = string.Join(",", Verbs);

        }

        public override string ToString() => Name;
    }
}