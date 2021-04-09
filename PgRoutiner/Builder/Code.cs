using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public abstract class Code
    {
        protected string I1 => string.Join("", Enumerable.Repeat(" ", settings.Ident));
        protected string I2 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 2));
        protected string I3 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 3));
        protected string I4 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 4));
        protected string I5 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 5));
        protected readonly string NL = Environment.NewLine;
        protected readonly Settings settings;

        public string Name { get; }
        public Dictionary<string, StringBuilder> Models { get; private set; } = new();
        public HashSet<string> UserDefinedModels { get; private set; } = new();
        public Dictionary<string, StringBuilder> ModelContent { get; private set; } = new();
        public StringBuilder Class { get; } = new();
        public List<Method> Methods { get; } = new();

        public Code(Settings settings, string name)
        {
            this.settings = settings;
            Name = name;
        }
    }
}
