﻿using System;
using System.Collections.Generic;
using System.Linq;
using VKApiCodeGen.Extensions;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Generator.Entities
{
    public class CSharpEnum : ISyntaxEntity
    {
        public string Name { get; set; }

        public CSharpSummary Summary { get; set; }

        public IDictionary<string, string> Keys { get; set; }

        public static CSharpEnum FromObject(IApiEnumEntity obj)
        {
            IDictionary<string, string> keys;

            if (obj.EnumNames != null)
            {
                var isNumberEnum = obj.Enum.All(v => char.IsNumber(v[0]));

                keys = obj.Enum
                    .Zip(obj.EnumNames, (val, name) => new KeyValuePair<string, string>(name.ToBeautifiedName(), val))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            else
            {
                keys = obj.Enum.ToDictionary(v => v.ToBeautifiedName());
            }

            return new CSharpEnum
            {
                Name = obj.Name?.ToBeautifiedName(),
                Summary = new CSharpSummary(obj.Description),
                Keys = keys
            };
        }

        public void WriteSyntax(SyntaxBuilder builder)
        {
            if (Summary != null)
            {
                Summary.WriteSyntax(builder);
            }

            builder.Line($"public enum {Name}");
            builder.Block(() =>
            {
                foreach (var key in Keys)
                {
                    if (int.TryParse(key.Value, out var intValue))
                    {
                        builder.Line($"{key.Key} = {key.Value},");
                    }
                    else
                    {
                        new CSharpAttribute($"EnumMember(Value = \"{key.Value}\")").WriteSyntax(builder);
                        builder.Line($"{key.Key},");
                    }
                }
            });
        }
    }
}
