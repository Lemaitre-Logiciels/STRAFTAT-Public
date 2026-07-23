#if UNITY_EDITOR
// Copyright 2019 The Gamedev Guru (http://thegamedev.guru)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheGamedevGuru
{
    public class EditorInstanceCreator : EditorWindow
    {
        string _projectInstanceName;
        string _extraSubdirectories = "";
        bool _includeProjectSettings = true;

        [MenuItem("Window/The Gamedev Guru/Editor Instance Creator")]
        static void Init()
        {
            ((EditorInstanceCreator)EditorWindow.GetWindow(typeof(EditorInstanceCreator))).Show();
        }

        void OnGUI()
        {
            if (string.IsNullOrEmpty(_projectInstanceName))
            {
                _projectInstanceName = PlayerSettings.productName + "_Slave_1";
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("The Gamedev Guru - Project Instance Creator");
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Slave Project Name");
            _projectInstanceName = EditorGUILayout.TextField("", _projectInstanceName);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Include Project Settings? (Recommended)");
            _includeProjectSettings = EditorGUILayout.Toggle("", _includeProjectSettings);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Extra Subdirectories? (Separate by comma)");
            _extraSubdirectories = EditorGUILayout.TextField("", _extraSubdirectories);
            EditorGUILayout.Separator();

            if (GUILayout.Button("Create"))
            {
                CreateProjectInstance(_projectInstanceName, _includeProjectSettings, _extraSubdirectories);
            }

            if (GUILayout.Button("Help"))
            {
                Application.OpenURL("https://thegamedev.guru/multiple-unity-editor-instances-within-a-single-project/");
            }
        }

        static void CreateProjectInstance(string projectInstanceName, bool includeProjectSettings, string extraSubdirectories)
        {
            var targetDirectory = Path.Combine(Directory.GetCurrentDirectory(), ".." + Path.DirectorySeparatorChar, projectInstanceName);
            Debug.Log(targetDirectory);
            if (Directory.Exists(targetDirectory))
            {
                EditorUtility.DisplayDialog("Error", $"Directory already exists: {targetDirectory}", "Ok :(");
                return;
            }

            Directory.CreateDirectory(targetDirectory);

            List<string> subdirectories = new List<string> { "Assets", "Packages" };
            if (includeProjectSettings)
            {
                subdirectories.Add("ProjectSettings");
            }

            foreach (var extraSubdirectory in extraSubdirectories.Split(','))
            {
                subdirectories.Add(extraSubdirectory.Trim());
            }

            foreach (var subdirectory in subdirectories)
            {
                var args = GetLinkCommand(subdirectory, targetDirectory);
                System.Diagnostics.Process.Start("CMD.exe", args);
            }

            EditorUtility.RevealInFinder(targetDirectory + Path.DirectorySeparatorChar + "Assets");
            EditorUtility.DisplayDialog("Done!", $"Done! Feel free to add it as an existing project at: {targetDirectory}", "Ok :)");
        }

        static string GetLinkCommand(string subdirectory, string targetDirectory)
        {
            return $"/c mklink /J \"{targetDirectory}{Path.DirectorySeparatorChar}{subdirectory}\" \"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}{subdirectory}\"";
        }
    }
}
#endif