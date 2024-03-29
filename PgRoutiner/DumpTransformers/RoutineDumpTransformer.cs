﻿using System;
using PgRoutiner.Builder.CodeBuilders;
using PgRoutiner.DataAccess.Models;
using static Npgsql.PostgresTypes.PostgresCompositeType;


namespace PgRoutiner.DumpTransformers;

public class RoutineDumpTransformer : DumpTransformer
{
    public PgItem Item { get; }
    const string returnsTableLine = "RETURNS TABLE";
    const string funcLine = "FUNCTION ";
    const string procLine = "PROC ";

    public RoutineDumpTransformer(PgItem item, List<string> lines) : base(lines)
    {
        this.Item = item;
    }

    public RoutineDumpTransformer BuildLines(
        string paramsString = null,
        bool dbObjectsCreateOrReplace = false,
        bool ignorePrepend = false,
        Action<string> lineCallback = null)
    {
        Prepend.Clear();
        Create.Clear();
        Append.Clear();

        if (lineCallback == null)
        {
            lineCallback = s => { };
        }

        bool isComment = false;
        bool isPrepend = true;
        bool isCreate = false;
        bool isAppend = true;

        var name1 = $"{Item.TypeName} {Item.Schema}.{Item.Name}{paramsString ?? "("}";
        var name2 = $"{Item.TypeName} {Item.Schema}.\"{Item.Name}\"{paramsString ?? "("}";
        var name3 = $"{Item.TypeName} \"{Item.Schema}\".\"{Item.Name}\"{paramsString ?? "("}";
        var name4 = $"{Item.TypeName} \"{Item.Schema}\".{Item.Name}{paramsString ?? "("}";

        var startSequence1 = $"CREATE {name1}";
        var startSequence2 = $"CREATE {name2}";
        var startSequence3 = $"CREATE {name3}";
        var startSequence4 = $"CREATE {name4}";

        var skip1 = $"execute {Item.TypeName.ToLowerInvariant()} {Item.Name.ToLowerInvariant()}(";
        var skip2 = $"execute {Item.TypeName.ToLowerInvariant()} \"{Item.Name.ToLowerInvariant()}\"(";
        var skip3 = $"execute {Item.TypeName.ToLowerInvariant()} {Item.Schema.ToLowerInvariant()}.{Item.Name.ToLowerInvariant()}(";
        var skip4 = $"execute {Item.TypeName.ToLowerInvariant()} {Item.Schema.ToLowerInvariant()}.\"{Item.Name.ToLowerInvariant()}\"(";
        var skip5 = $"execute {Item.TypeName.ToLowerInvariant()} \"{Item.Schema.ToLowerInvariant()}\".\"{Item.Name.ToLowerInvariant()}\"(";
        var skip6 = $"execute {Item.TypeName.ToLowerInvariant()} \"{Item.Schema.ToLowerInvariant()}\".{Item.Name.ToLowerInvariant()}(";

        string statement = "";
        string endSequence = null;

        foreach (var l in lines)
        {
            var line = l;
            if (!isCreate && (line.StartsWith("--") || line.StartsWith("SET ") || line.StartsWith("SELECT ")))
            {
                continue;
            }
            if (!isCreate && isComment && line.Contains("*/"))
            {
                isComment = false;
            }
            if (!isCreate && isComment)
            {
                continue;
            }
            if (!isCreate && line.Contains("/*") && !line.Contains("*/"))
            {
                isComment = true;
            }
            if (!isCreate && string.IsNullOrEmpty(statement) && 
                !line.Contains(name1) && 
                !line.Contains(name2) && 
                !line.Contains(name3) && 
                !line.Contains(name4))
            {
                continue;
            }
            
            if (!isCreate)
            {
                var lower = line.ToLowerInvariant();
                if (lower.Contains(skip1) || 
                    lower.Contains(skip2) || 
                    lower.Contains(skip3) || 
                    lower.Contains(skip4) ||
                    lower.Contains(skip5) ||
                    lower.Contains(skip6))
                {
                    continue;
                }
            }
            
            var createStart = line.StartsWith(startSequence1) || line.StartsWith(startSequence2) || line.StartsWith(startSequence3) || line.StartsWith(startSequence4);
            var createEnd = endSequence != null && line.Contains($"{endSequence};");
            if (createStart)
            {
                if (dbObjectsCreateOrReplace)
                {
                    line = line.Replace("CREATE", "CREATE OR REPLACE");
                }
                isPrepend = false;
                isCreate = true;
                isAppend = false;
                if (Create.Count > 0)
                {
                    Create.Add("");
                }
                if (!line.Contains("()"))
                {
                    var pIndex = line.IndexOf("(");
                    Create.Add(line.Substring(0, pIndex + 1));
                    var parameters = line.Substring(pIndex + 1, line.IndexOf(')', pIndex) - pIndex - 1).Split(',');
                    foreach (var (p, index) in parameters.Select((p, i) => (p.Trim(), i)))
                    {
                        Create.Add(string.Concat(Code.GetIdent(1), p, index < parameters.Length - 1 ? ',' : "")); ;
                    }
                    Create.Add(")");
                    line = line.Substring(line.IndexOf(')', pIndex) + 2);
                }
            }
            if (isCreate)
            {
                if (endSequence == null)
                {
                    endSequence = line.GetSequence();
                    if (endSequence != null)
                    {
                        if (line.IsRepeatedSubstring(endSequence, 2))
                        {
                            createEnd = true;
                        }
                    }
                }
                if (endSequence == null && line.Contains("RETURN "))
                {
                    endSequence = "";
                    createEnd = line.Contains($";");
                }
                if (endSequence == null && line.Contains("BEGIN"))
                {
                    endSequence = "END";
                }
                
                var returnsTableIndex = line.IndexOf(returnsTableLine);
                if (returnsTableIndex == -1)
                {
                    if (line.Contains("LANGUAGE") || line.Contains("AS $"))
                    {
                        line = line.Trim();
                    }
                    Create.Add(line);
                }
                else
                {
                    returnsTableIndex = line.IndexOf("(", returnsTableIndex + returnsTableLine.Length);
                    var original = line;
                    line = original.Substring(0, returnsTableIndex);
                    var expression = original.Substring(returnsTableIndex);
                    
                    Create.Add(string.Concat(line, '('));
                    var fieldsExp = expression.Between('(', ')', useLastIndex: true);
                    var fields = fieldsExp.Split(", ");

                    foreach (var (field, index) in fields.Select((f,i) => (f,i)))
                    {
                        Create.Add(string.Concat(Code.GetIdent(1), field, index < fields.Length - 1 ? ',' : ""));
                    }
                    Create.Add(expression.Substring(expression.LastIndexOf(')')));
                }
                
                if (createEnd)
                {
                    isPrepend = false;
                    isCreate = false;
                    isAppend = true;
                }
                if (!createStart && !createEnd && !isAppend)
                {
                    lineCallback(line);
                }
            }
            else
            {
                statement = string.Concat(statement, statement == "" ? "" : Environment.NewLine, line);
                if (statement.EndsWith(";"))
                {
                    if (isPrepend && !ignorePrepend)
                    {
                        Prepend.Add(statement);
                    }
                    else if (isAppend)
                    {
                        Append.Add(statement);
                    }
                    statement = "";
                }
            }
        }
        return this;
    }
}
