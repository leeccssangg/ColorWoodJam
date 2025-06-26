using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

namespace BW.Inspector
{
// Tags, Layers and Scene Builder - Auto Generate Tags, Layers and Scenes classes containing consts for all variables for code completion - 2012-10-01
// released under MIT License
// http://www.opensource.org/licenses/mit-license.php
//
//@author		Devin Reimer - AlmostLogical Software / Owlchemy Labs
//@website 		http://blog.almostlogical.com, http://owlchemylabs.com
/*
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
//Note: This class uses UnityEditorInternal which is an undocumented internal feature
    public class TagsLayersScenesBuilder : EditorWindow
    {
        private const string FOLDER_LOCATION = "_Modules/_Game/Scripts/";
        private const string TAGS_FILE_NAME = "Tags";
        private const string LAYERS_FILE_NAME = "Layers";
        private const string SCENES_FILE_NAME = "Scenes";
        private const string SCRIPT_EXTENSION = ".cs";

        [MenuItem("Edit/Rebuild Tags, Layers and Scenes Classes")]
        public static void RebuildTagsAndLayersClasses()
        {
            RebuildTags();
            RebuildLayers();
            RebuildScenes();

            Debug.Log("Rebuild Complete");
        }

        public static void RebuildTags()
        {
            string folderPath = Application.dataPath + "/" + FOLDER_LOCATION;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            try
            {
                // Remove category
//                string[] tags = InternalEditorUtility.tags;
//
//                for (int i = 0; i < tags.Length; i++)
//                {
//                    string[] splits = tags[i].Split('/');
//
//                    if (splits.Length == 2)
//                    {
//                        tags[i] = splits[1];
//                    }
//                }

                File.WriteAllText(folderPath + TAGS_FILE_NAME + SCRIPT_EXTENSION, GetTagClassContent(TAGS_FILE_NAME, InternalEditorUtility.tags));
                AssetDatabase.ImportAsset("Assets/" + FOLDER_LOCATION + TAGS_FILE_NAME + SCRIPT_EXTENSION, ImportAssetOptions.ForceUpdate);
            }
            catch (Exception)
            {
                Debug.LogError("Generate tags failed");
            }
        }

        public static void RebuildLayers()
        {
            string folderPath = Application.dataPath + "/" + FOLDER_LOCATION;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            try
            {
                File.WriteAllText(folderPath + LAYERS_FILE_NAME + SCRIPT_EXTENSION, GetLayerClassContent(LAYERS_FILE_NAME, InternalEditorUtility.layers));
                AssetDatabase.ImportAsset("Assets/" + FOLDER_LOCATION + LAYERS_FILE_NAME + SCRIPT_EXTENSION, ImportAssetOptions.ForceUpdate);
            }
            catch (Exception)
            {
                Debug.LogError("Generate layers failed");
            }
        }

        public static void RebuildScenes()
        {
            string folderPath = Application.dataPath + "/" + FOLDER_LOCATION;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            try
            {
                File.WriteAllText(folderPath + SCENES_FILE_NAME + SCRIPT_EXTENSION,
                    GetClassContent(SCENES_FILE_NAME, EditorBuildSettingsScenesToNameStrings(EditorBuildSettings.scenes)));
                AssetDatabase.ImportAsset("Assets/" + FOLDER_LOCATION + SCENES_FILE_NAME + SCRIPT_EXTENSION, ImportAssetOptions.ForceUpdate);
            }
            catch (Exception)
            {
                Debug.LogError("Generate scenes failed");
            }
        }

        private static string[] EditorBuildSettingsScenesToNameStrings(EditorBuildSettingsScene[] scenes)
        {
            string[] sceneNames = new string[scenes.Length];
            for (int n = 0; n < sceneNames.Length; n++)
            {
                sceneNames[n] = Path.GetFileNameWithoutExtension(scenes[n].path);
            }

            return sceneNames;
        }

        private static string GetTagClassContent(string className, string[] labelsArray)
        {
            string output = "";
            output += "//This class is auto-generated do not modify (TagsLayersScenesBuilder.cs)\n";
            output += "public class " + className + "\n";
            output += "{\n";
            foreach (string label in labelsArray)
            {
                output += "\t" + BuildTagConstVariable(label) + "\n";
            }

            output += "}";
            return output;
        }

        private static string GetClassContent(string className, string[] labelsArray)
        {
            string output = "";
            output += "//This class is auto-generated do not modify (TagsLayersScenesBuilder.cs)\n";
            output += "public class " + className + "\n";
            output += "{\n";
            foreach (string label in labelsArray)
            {
                output += "\t" + BuildConstVariable(label) + "\n";
            }

            output += "}";
            return output;
        }

        private static string GetLayerClassContent(string className, string[] labelsArray)
        {
            string output = "";
            output += "//This class is auto-generated do not modify (TagsLayersScenesBuilder.cs)\n";
            output += "public class " + className + "\n";
            output += "{\n";
            foreach (string label in labelsArray)
            {
                output += "\t" + BuildConstVariable(label) + "\n";
            }

            output += "\n";

            foreach (string label in labelsArray)
            {
                output += "\t" + "public const int " + ToUpperCaseWithUnderscores(label) + "_Int" + " = " + LayerMask.NameToLayer(label) + ";\n";
            }

            output += "}";
            return output;
        }

        private static string BuildTagConstVariable(string varName)
        {
            // Remove category prefix
            string varValue = varName;

            string[] splits = varValue.Split('/');

            if (splits.Length == 2)
            {
                varValue = splits[1];
            }

            return "public const string " + RemoveSlash(ToUpperCaseWithUnderscores(varName)) + " = " + '"' + varValue + '"' + ";";
        }

        private static string BuildConstVariable(string varName)
        {
            return "public const string " + RemoveSlash(ToUpperCaseWithUnderscores(varName)) + " = " + '"' + varName + '"' + ";";
        }

        private static string RemoveSlash(string input)
        {
            return input.Replace("/", "_");
        }

        private static string ToUpperCaseWithUnderscores(string input)
        {
            string output = "" + input[0];

            for (int n = 1; n < input.Length; n++)
            {
//                if ((char.IsUpper(input[n]) || input[n] == ' ') && !char.IsUpper(input[n - 1]) && input[n - 1] != '_' && input[n - 1] != ' ')
//                {
//                    output += "_";
//                }

                if (input[n] != ' ' && input[n] != '_')
                {
                    output += input[n];
                }
            }

//            output = output.ToUpper();
            return output;
        }
    }
}
#endif