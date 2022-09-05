using Microsoft.Extensions.Configuration;


namespace PgRoutiner.Connection;

public partial class ConnectionManager
{
    private readonly IConfigurationRoot config;
    private readonly string connectionKey;
    private readonly string skipKey;
    private readonly string name;
    private readonly string name1;
    private readonly string name2;

    public ConnectionManager(IConfigurationRoot config,
        string connectionKey = nameof(Current.Value.Connection),
        string skipKey = null)
    {
        this.config = config;
        this.connectionKey = connectionKey;
        this.skipKey = skipKey;
        this.name = typeof(Current).GetProperty(connectionKey).GetValue(Current.Value) as string;
        var key = connectionKey.ToKebabCase().Replace("-", " ");
        if (key == "connection")
        {
            this.name1 = "connection";
            this.name2 = "Connection";
        }
        else
        {
            this.name1 = $"{key} connection";
            this.name2 = $"{$"{char.ToUpper(key[0])}{key[1..]}"} connection";
        }
    }
}
