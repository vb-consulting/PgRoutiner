using System.Data;
using NpgsqlTypes;

namespace PgRoutiner.DataAccess;

public static class _Orm
{
    public static IEnumerable<T> Read<T>(this NpgsqlConnection connection,
        IEnumerable<(object value, DbType? dbType, NpgsqlDbType? npgsqlDbType)> parameters,
        string command, 
        Func<NpgsqlDataReader, T> action)
    {
        if (connection.State != ConnectionState.Open) connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = command;
        foreach(var (value, dbType, npgsqlDbType) in parameters)
        {
            var p = cmd.CreateParameter();
            p.Value = value ?? DBNull.Value;
            if (dbType.HasValue) p.DbType = dbType.Value;
            if (npgsqlDbType.HasValue) p.NpgsqlDbType = npgsqlDbType.Value;
            cmd.Parameters.Add(p);
        }
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) 
        {
            yield return action(reader);
        }
        reader.Close();
    }

    public static void Execute(this NpgsqlConnection connection, string command)
    {
        if (connection.State != ConnectionState.Open) connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = command;
        cmd.ExecuteNonQuery(); 
    }

    public static T Val<T>(this NpgsqlDataReader reader, string name)
    { 
        var value = reader[name];
        return value == DBNull.Value ? default : (T)value;
    }

    public static T Val<T>(this NpgsqlDataReader reader, int ordinal)
    {
        var value = reader[ordinal];
        return value == DBNull.Value ? default : (T)value;
    }
}
