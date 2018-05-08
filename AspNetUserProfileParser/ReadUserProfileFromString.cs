using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

public partial class UserDefinedFunctions
{
    private class ParsedProperty
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }

    readonly static Regex propertyNameRegex = new Regex(@"(?<Name>[a-zA-Z0-9]+):(?<Type>[SB]):(?<Start>[0-9]+):(?<Length>[0-9]+):");

    static string ParseSerializedData(byte[] array)
    {
        var f = new BinaryFormatter();
        string Escape(string org)
        {
            var map = new Dictionary<string, string>
            {
                { "\"", "\\\"" },
                { "\\", "\\\\" },
                { "/", "\\/" },
                { "\b", "\\b" },
                { "\r", "\\r" },
                { "\n", "\\n" },
                { "\t", "\\t" },
            };
            foreach (var m in map)
            {
                org = org.Replace(m.Key, m.Value);
            }
            return org;
        }
        using (var ms = new MemoryStream(array, false))
        {
            var obj = f.Deserialize(ms);
            var dic = obj as Dictionary<string, string>;
            return string.Format("{{{0}}}",
                string.Join(",", dic.Select(kv => $@"""{Escape(kv.Key)}"": ""{Escape(kv.Value)}""")
                ));
        }

    }

    static string ReadValue(string propertyValues, string name, int start, int length)
    {
        return propertyValues.Substring(start, length);
    }

    static string ReadValue(byte[] propertyValues, string name, int start, int length)
    {
        switch (name)
        {
            case "SerializedData":
                var ba = propertyValues.Skip(start).Take(length).ToArray();
                return ParseSerializedData(ba);
            default:
                return null;
        }
    }

    [Microsoft.SqlServer.Server.SqlFunction(
        DataAccess = DataAccessKind.None,
        FillRowMethodName = nameof(ReadUserProfileFromString_FillRow),
        TableDefinition = "PropertyName NVARCHAR(MAX), PropertyValue NVARCHAR(MAX)"
        )]
    public static IEnumerable ReadUserProfileFromString(SqlString propertyNames, SqlString propertyValuesString, SqlBytes propertyValuesBytes)
    {
        var matchPropertyName = propertyNameRegex.Match(propertyNames.Value);
        var propertyValuesStringValue = propertyValuesString.Value;
        var propertyValuesImageValue = propertyValuesBytes.Value;

        while (matchPropertyName.Success)
        {
            var name = matchPropertyName.Groups["Name"].Value;
            var type = matchPropertyName.Groups["Type"].Value;
            var start = int.Parse(matchPropertyName.Groups["Start"].Value);
            var length = int.Parse(matchPropertyName.Groups["Length"].Value);
            matchPropertyName = matchPropertyName.NextMatch();

            switch (type)
            {
                case "S":
                    yield return new ParsedProperty
                    {
                        PropertyName = name,
                        PropertyValue = ReadValue(propertyValuesStringValue, name, start, length),
                    };
                    break;
                case "B":
                    yield return new ParsedProperty
                    {
                        PropertyName = name,
                        PropertyValue = ReadValue(propertyValuesImageValue, name, start, length),
                    };
                    break;
                default:
                    yield return new ParsedProperty
                    {
                        PropertyName = name,
                        PropertyValue = null,
                    };
                    break;
            }
        }
    }

    public static void ReadUserProfileFromString_FillRow(object resultObject, out SqlString propertyName, out SqlString propertyValue)
    {
        var p = (ParsedProperty)resultObject;
        propertyName = p.PropertyName;
        propertyValue = p.PropertyValue;
    }
}
