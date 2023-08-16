using System.IO;
using UnityEditor;
using UnityEngine;

namespace DoxygenGenerator
{
    public static class MenuItems
    {
        private struct PackageData
        {
            public string displayName;
            public string version;
        }

        // Add a menu item to generate a doxygen api from the menu command.
        [MenuItem("CONTEXT/DefaultAsset/Generate Doxygen API")]
        private static void GenerateDoxygenApi(MenuCommand menuCommand)
        {
            var assetPath = AssetDatabase.GetAssetPath(menuCommand.context);

            if (Directory.Exists(assetPath))
            {
                var directoryPath = new DirectoryInfo(assetPath).FullName;
                ParamGenerator.InputDirectory.Set(0, directoryPath);;

                var packageFiles = Directory.GetFiles(directoryPath, "package.json");
                if(packageFiles.Length > 0)
                {
                    var packageFilePath = packageFiles[0];
                    var packageJson = File.ReadAllText(packageFilePath);

                    var packageData = JsonUtility.FromJson<PackageData>(packageJson);
                    ParamGenerator.Project.Set(0, packageData.displayName);
                    ParamGenerator.Version.Set(0, packageData.version);
                }
            }

            GeneratorWindow.Initialize();
        }
    }
}
