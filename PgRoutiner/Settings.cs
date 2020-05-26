namespace PgRoutiner
{
    public class Settings
    {
        public string Connection { get; set; }
        public string Project { get; set; }
        public string OutputDir { get; set; } = "";
        public string Schema { get; set; } = "public";
        public bool SkipExisting { get; set; } = false;
        public string Namespace { get; set; }

        public static Settings Value { get; set; } = new Settings();
    }
}
