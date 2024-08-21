#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class EditorMethods : Editor
{
    const string extension = ".cs";
    /// <summary>
    /// Convert enum list to .cs and write to file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    public static void WriteToEnum<T>(string path, string name, ICollection<T> data)
    {
        using (StreamWriter file = File.CreateText(path + name + extension))
        {
            file.WriteLine("public enum " + name + " \n{");

            int i = 0;
            foreach (var line in data)
            {
                string lineRep = line.ToString().Replace(" ", string.Empty);
                if (!string.IsNullOrEmpty(lineRep))
                {
                    file.WriteLine(string.Format("\t{0} = {1},",
                        lineRep, i));
                    i++;
                }
            }

            file.WriteLine("}");
        }

        AssetDatabase.ImportAsset(path + name + extension);
    }
}

#endif

