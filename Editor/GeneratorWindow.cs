using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace DoxygenGenerator
{
    public class GeneratorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private Thread doxygenThread;
        private string generateButtonName = "Generate";

        private bool CanGenerate(int index) => File.Exists(ParamGenerator.DoxygenPath)
            && Directory.Exists(ParamGenerator.InputDirectory.Get(index))
            && Directory.Exists(ParamGenerator.OutputDirectory.Get(index))
            && doxygenThread == null;


        [MenuItem("Tools/Doxygen Generator")]
        public static void Initialize()
        {
            var window = GetWindow<GeneratorWindow>("Doxygen Generator");
            window.minSize = new Vector2(420, 245);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Select your doxygen install location
            DoxygenInstallPathGUI();

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < 2; i++)
            {
                EditorGUILayout.BeginVertical();
                // Setup the directories
                SetupTheDirectoriesGUI(i);

                // Set your project settings
                ProjectSettingsGUI(i);

                // Generate the API
                DocumentationGUI(i);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndScrollView();
        }

        private void DoxygenInstallPathGUI()
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            GUILayout.Label("Doxygen Install Path", EditorStyles.boldLabel);

            // Doxygen not selected error
            if (!File.Exists(ParamGenerator.DoxygenPath))
            {
                ParamGenerator.DoxygenPath = default;
                EditorGUILayout.HelpBox("No doxygen install path is selected. Please install Doxygen and select it below.", MessageType.Error, true);
                if (GUILayout.Button("Download Doxygen", GUILayout.MaxWidth(150)))
                {
                    Application.OpenURL("https://www.doxygen.nl/download.html");
                }
            }

            // Doxygen Path
            EditorGUILayout.BeginHorizontal();
            ParamGenerator.DoxygenPath = EditorGUILayout.DelayedTextField("doxygen.exe", ParamGenerator.DoxygenPath);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                ParamGenerator.DoxygenPath = EditorUtility.OpenFilePanel("Select your doxygen.exe", string.Empty, string.Empty);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void SetupTheDirectoriesGUI(int index)
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            GUILayout.Label("Setup the Directories", EditorStyles.boldLabel);
            // Input not selected error
            if (!Directory.Exists(GetInputPath()))
            {
                SetInputPath(default);
                EditorGUILayout.HelpBox("No input directory selected. Please select a directory you would like your API to be generated from.", MessageType.Error, true);
            }

            // Input Directory
            EditorGUILayout.BeginHorizontal();
            SetInputPath(EditorGUILayout.DelayedTextField("Input Directory", GetInputPath()));
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                SetInputPath(EditorUtility.OpenFolderPanel("Select your Input Directory", string.Empty, string.Empty));
            }
            EditorGUILayout.EndHorizontal();

            // Output not selected error
            if (!Directory.Exists(GetOutputPath()))
            {
                SetOutputPath(default);
                EditorGUILayout.HelpBox("No output directory selected. Please select a directory you would like your API to be generated to.", MessageType.Error, true);
            }

            // Output Directory
            EditorGUILayout.BeginHorizontal();
            SetOutputPath(EditorGUILayout.DelayedTextField("Output Directory", GetOutputPath()));
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                SetOutputPath(EditorUtility.OpenFolderPanel("Select your Output Directory", string.Empty, string.Empty));
            }
            EditorGUILayout.EndHorizontal();
            
            string GetInputPath()            => ParamGenerator.InputDirectory.Get(index);
            void   SetInputPath(string path) => ParamGenerator.InputDirectory.Set(index, path);

            string GetOutputPath()            => ParamGenerator.OutputDirectory.Get(index);
            void   SetOutputPath(string path) => ParamGenerator.OutputDirectory.Set(index, path);
        }

        private void ProjectSettingsGUI(int index)
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            GUILayout.Label("Project Settings", EditorStyles.boldLabel);
            ParamGenerator.Project.Set(index, EditorGUILayout.TextField("Name", ParamGenerator.Project.Get(index)));
            ParamGenerator.Synopsis.Set(index, EditorGUILayout.TextField("Synopsis", ParamGenerator.Synopsis.Get(index)));
            ParamGenerator.Version.Set(index, EditorGUILayout.TextField("Version", ParamGenerator.Version.Get(index)));
        }

        private void DocumentationGUI(int index)
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            GUILayout.Label("Documentation", EditorStyles.boldLabel);

            // Update the doxygen thread
            if(doxygenThread != null)
            {
                switch (doxygenThread.ThreadState)
                {
                    case ThreadState.Aborted:
                    case ThreadState.Stopped:
                        doxygenThread = null;
                        generateButtonName = "Generate";
                        break;
                }
            }

            // Generate Button
            EditorGUI.BeginDisabledGroup(!CanGenerate(index));
            if (GUILayout.Button(generateButtonName, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                doxygenThread = Generator.GenerateAsync(index);
                generateButtonName = "Generating...";
            }
            EditorGUI.EndDisabledGroup();

            // Open Button
            EditorGUI.BeginDisabledGroup(!Directory.Exists(GetOutputPath()) || doxygenThread != null);
            if (GUILayout.Button("Open", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                System.Diagnostics.Process.Start(GetOutputPath());
            }
            EditorGUI.EndDisabledGroup();

            // View Log Button
            var logPath = $"{GetOutputPath()}/Log.txt";
            EditorGUI.BeginDisabledGroup(!File.Exists(logPath) || doxygenThread != null);
            if (GUILayout.Button("View Log", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                Application.OpenURL($"File://{logPath}");
            }
            EditorGUI.EndDisabledGroup();

            // Browse Button
            var browsePath = $"{GetOutputPath()}/html/annotated.html";
            EditorGUI.BeginDisabledGroup(!File.Exists(browsePath) || doxygenThread != null);
            if (GUILayout.Button("Browse", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                Application.OpenURL($"File://{browsePath}");
            }
            EditorGUI.EndDisabledGroup();
            
            string GetOutputPath()            => ParamGenerator.OutputDirectory.Get(index);
        }
    }
}
