#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

public class EnumEdits : MonoBehaviour
{
    [Serializable]
    public class Enums
    {
        public string EnumName;
        public Color GUIColor = Color.white;

        public List<string> IDs = new List<string>();

        [HideInInspector] public bool Active;
    }

    public List<Enums> enums = new List<Enums>();

    public bool CheckEnumList(List<string> list)
    {
        if (list.Count <= 0)
            return false;

        for (int i = 0; i < list.Count; i++)
        {
            if (string.IsNullOrEmpty(list[i]))
                return false;

            for (int j = 0; j < list.Count; j++)
            {
                if (i != j)
                {
                    if (list[i] == list[j])
                        return false;
                }
            }
        }

        return true;
    }

    public Enums GetDefaultEnums()
    {
        Enums tempEnums = new Enums();
        tempEnums.EnumName = "EnumDefault";
        tempEnums.Active = true;

        return tempEnums;
    }

    public bool CheckString(string text)
    {
        char[] chars = text.ToCharArray();

        foreach (var item in chars)
        {
            if (item == ' ') return false;
        }

        return true;
    }
}

[CustomEditor(typeof(EnumEdits))]
public class EditorEnumEdits : Editor
{
    EnumEdits enumEdits;
    string filePath = "Assets/_Project/Scripts/Enums/";

    Color defaultBGColor;

    SerializedProperty tempEnums;

    ReorderableList reorderableList;

    private void OnEnable()
    {
        tempEnums = serializedObject.FindProperty("enums");

        /*reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("enums"), true, true, true, true);
        reorderableList.drawElementCallback = DrawListItems;
        reorderableList.drawHeaderCallback = DrawHeader;*/
    }

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        /*reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - 60, rect.height), "State");
        };*/

        SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index); //The element in the list

        // Create a property field and label field for each property. 

        // The 'mobs' property. Since the enum is self-evident, I am not making a label field for it. 
        // The property field for mobs (width 100, height of a single line)
        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("IDs"),
            GUIContent.none
        );

        // The 'level' property
        // The label field for level (width 100, height of a single line)
        EditorGUI.LabelField(new Rect(rect.x + 120, rect.y, 350, EditorGUIUtility.singleLineHeight), "Enum Name");

        //The property field for level. Since we do not need so much space in an int, width is set to 20, height of a single line.
        EditorGUI.PropertyField(
            new Rect(rect.x + 160, rect.y, 20, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("EnumName"),
            GUIContent.none
        );
    }

    void DrawHeader(Rect rect)
    {
        string name = "IDs";
        EditorGUI.LabelField(rect, name);
    }

    public override void OnInspectorGUI()
    {
        defaultBGColor = GUI.backgroundColor;

        serializedObject.Update();

        enumEdits = (EnumEdits)target;

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("ENUM EDIT SYSTEM", MessageType.None);

        //GUI.color = defaultBGColor;
        if (enumEdits.enums.Count <= 0)
        {
            base.OnInspectorGUI();
        }

        //EditorGUILayout.BeginHorizontal();

        if (enumEdits.enums.Count > 0)
        {
            //enumEdits.enums.Add(enumEdits.GetDefaultEnums());

            for (int i = 0; i < enumEdits.enums.Count; i++)
            {
                if (enumEdits.enums[i].Active)
                    GUI.backgroundColor = enumEdits.enums[i].GUIColor;
                else
                    GUI.backgroundColor = Color.white;

                if (i % 4 == 0 && i != 0)
                    EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(enumEdits.enums[i].EnumName))
                    enumEdits.enums[i].Active = EnableCategory();

                if (i % 4 == 0 && i != 0)
                    EditorGUILayout.EndHorizontal();
            }

            bool check = false;
            for (int i = 0; i < enumEdits.enums.Count; i++)
            {
                if (enumEdits.enums[i].Active)
                    check = true;
            }

            if (!check)
                enumEdits.enums[0].Active = EnableCategory();
        }

        //EditorGUILayout.EndHorizontal();

        for (int i = 0; i < enumEdits.enums.Count; i++)
        {
            if (enumEdits.enums[i].Active)
            {
                if (enumEdits.enums[i].GUIColor.a <= 0)
                    enumEdits.enums[i].GUIColor = Color.white;

                GUI.color = enumEdits.enums[i].GUIColor;

                //reorderableList.DoLayoutList();

                EditorGUILayout.PropertyField(tempEnums, true);

                //EditorGUILayout.PropertyField(tempList);

                GUI.color = Color.cyan;
                filePath = EditorGUILayout.TextField("Path", filePath);

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                if (enumEdits.CheckEnumList(enumEdits.enums[i].IDs) && enumEdits.CheckString(enumEdits.enums[i].EnumName))
                    GUI.color = Color.green;
                else
                    GUI.color = Color.red;

                if (GUILayout.Button("Save", GUILayout.Width(350f), GUILayout.Height(35f)))
                {
                    if (enumEdits.CheckEnumList(enumEdits.enums[i].IDs) && enumEdits.CheckString(enumEdits.enums[i].EnumName))
                    {
                        EditorMethods.WriteToEnum(filePath, enumEdits.enums[i].EnumName, enumEdits.enums[i].IDs);
                    }
                }

                GUI.color = enumEdits.enums[i].GUIColor;
            }
        }

        //base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed && !EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(enumEdits);
        }
    }

    bool EnableCategory()
    {
        for (int i = 0; i < enumEdits.enums.Count; i++)
        {
            enumEdits.enums[i].Active = false;
        }

        return true;

    }
}

#endif
