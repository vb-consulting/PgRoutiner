﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Npgsql;

namespace PgRoutiner
{
    partial class Builder
    {
        public static List<(List<Method> Methods, string @namespace)> Modules = new();
        public static List<(string Content, string FullFileName)> Content = new();
        public static string SchemaFile = null;
        public static string DataFile = null;

        public static void Run(string connectionStr)
        {
            using var connection = new NpgsqlConnection(connectionStr);

            BuildDataAccess(connection);
            DumpContent();

            var builder = new PgDumpBuilder(Settings.Value, connection);
            BuildDump(Settings.Value.SchemaDumpFile, () => builder.GetSchemaContent(), file => SchemaFile = file);
            BuildDump(Settings.Value.DataDumpFile, () => builder.GetDataContent(), file => DataFile = file);
            BuildObjectDumps(builder);

            Dump("Done!");
        }

        private static void DumpContent()
        {
            if (Content.Count == 0)
            {
                return;
            }
            Dump("Creating source code files...");
            foreach (var item in Content)
            {
                File.WriteAllText(item.FullFileName, item.Content);
            }
        }

        private static void Dump(params string[] lines)
        {
            Program.WriteLine(ConsoleColor.Yellow, lines);
        }

        private static void Error(string msg)
        {
            Program.WriteLine(ConsoleColor.Red, $"ERROR: {msg}");
        }
    }
}
