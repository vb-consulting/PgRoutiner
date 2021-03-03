﻿using System;
using System.Linq;

namespace PgRoutiner
{
    public abstract class CodeHelpers
    {
        protected string I1 => string.Join("", Enumerable.Repeat(" ", settings.Ident));
        protected string I2 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 2));
        protected string I3 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 3));
        protected string I4 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 4));
        protected string I5 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 5));
        protected readonly string NL = Environment.NewLine;
        protected readonly Settings settings;

        public CodeHelpers(Settings settings)
        {
            this.settings = settings;
        }
    }
}
