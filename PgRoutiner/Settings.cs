namespace PgRoutiner
{
    public class Settings
    {
        public string Connection { get; set; }
        public string Project { get; set; }
        public string OutputDir { get; set; } = "";
        public string Schema { get; set; } = "public";
        public bool Overwrite { get; set; } = true;
        public string Namespace { get; set; }
        public string SkipSimilarTo { get; set; }

        public static Settings Value { get; set; } = new Settings();
    }
}
