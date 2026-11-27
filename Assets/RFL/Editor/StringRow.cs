#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace RFL.Editor
{
    public readonly struct StringRow
    {
        public readonly int Line;
        public readonly string Key;
        public readonly string Id;
        public readonly string En;

        public StringRow(int line, string key, string id, string en)
        {
            Line = line;
            Key = key;
            Id = id;
            En = en;
        }
    }
}
