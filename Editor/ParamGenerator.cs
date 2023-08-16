using UnityEditor;

namespace DoxygenGenerator
{
    public static class ParamGenerator
    {
        public static string DoxygenPath
        {
            get => EditorPrefs.GetString($"{nameof(DoxygenGenerator)}.{nameof(ParamGenerator)}.{nameof(DoxygenPath)}", string.Empty);
            set => EditorPrefs.SetString($"{nameof(DoxygenGenerator)}.{nameof(ParamGenerator)}.{nameof(DoxygenPath)}", value);
        }

        public class DoxygenPrefsParam
        {
            private readonly string name;
            private readonly string defaultPath = string.Empty;
            private string StringId(int index) => $"{nameof(DoxygenGenerator)}.{nameof(ParamGenerator)}.{name}.{index}";

            public string Get(int index)              => EditorPrefs.GetString(StringId(index), defaultPath);
            public void   Set(int index, string path) => EditorPrefs.SetString(StringId(index), path);

            public DoxygenPrefsParam(string directoryName) => name  = directoryName;
        }

        public static DoxygenPrefsParam InputDirectory  = new(nameof(InputDirectory));
        public static DoxygenPrefsParam OutputDirectory = new(nameof(OutputDirectory));

        public static DoxygenPrefsParam Project  = new(nameof(Project));
        public static DoxygenPrefsParam Synopsis = new(nameof(Synopsis));
        public static DoxygenPrefsParam Version  = new(nameof(Version));
    }
}
