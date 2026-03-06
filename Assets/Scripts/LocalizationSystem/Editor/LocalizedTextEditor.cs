using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using KProject.LocalizationSystem;

namespace KProject.LocalizationSystem.Editor
{
    [CustomEditor(typeof(LocalizedText))]
    public class LocalizedTextEditor : UnityEditor.Editor
    {
        private List<string> _keys = new List<string>();
        private int _selectedIndex = 0;

        void OnEnable()
        {
            LoadKeys();
            string currentKey = serializedObject.FindProperty("key").stringValue;
            _selectedIndex = _keys.IndexOf(currentKey);
            if (_selectedIndex < 0) _selectedIndex = 0;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty keyProp = serializedObject.FindProperty("key");

            if (_keys.Count == 0)
            {
                EditorGUILayout.HelpBox("No keys found. Check StreamingAssets/Localization/en.json", MessageType.Warning);
                EditorGUILayout.PropertyField(keyProp);
            }
            else
            {
                EditorGUILayout.LabelField("Key", EditorStyles.boldLabel);

                int newIndex = EditorGUILayout.Popup(_selectedIndex, _keys.ToArray());

                if (newIndex != _selectedIndex)
                {
                    _selectedIndex = newIndex;
                    keyProp.stringValue = _keys[_selectedIndex];
                }

                EditorGUILayout.LabelField("Selected Key", keyProp.stringValue, EditorStyles.helpBox);

                EditorGUILayout.Space(4);
                if (GUILayout.Button("Refresh Keys"))
                {
                    LoadKeys();
                    _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _keys.Count - 1);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void LoadKeys()
        {
            _keys.Clear();
            string path = Path.Combine(Application.streamingAssetsPath, "Localization", "en.json");

            if (!File.Exists(path))
                return;

            string json = File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<LocalizationWrapper>(json);

            if (wrapper?.entries == null) return;

            foreach (var entry in wrapper.entries)
                _keys.Add(entry.key);
        }

        [System.Serializable]
        private class LocalizationWrapper
        {
            public List<LocalizationEntry> entries;
        }

        [System.Serializable]
        private class LocalizationEntry
        {
            public string key;
            public string value;
        }
    }
}
