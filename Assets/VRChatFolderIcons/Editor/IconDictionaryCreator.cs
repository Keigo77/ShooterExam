using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace VRChatFolderIcons.Editor
{
    [InitializeOnLoad]
    public class IconDictionaryCreator : AssetPostprocessor
    {
        internal static bool IsEnable;
        internal static Dictionary<Regex, Texture> IconDictionary;

        static IconDictionaryCreator()
        {
            FindAndBuildDictionary();
        }
        
        internal static void BuildDictionary(FolderIconSettingScriptableObject settings)
        {
            var dictionary = new Dictionary<Regex, Texture>();

            IsEnable = settings.enable;
            if (settings == null || !IsEnable) return;

            foreach (var setting in settings.folderIconSettings)
            {
                if (setting.icon == null || !setting.enable) continue;
                
                var texture = (Texture)setting.icon;
                var regexString = setting.regex;
                
                if(setting.matchType == MatchType.FullMatch)
                    regexString = $"^{regexString}$";

                try
                {
                    var regex = new Regex(regexString, RegexOptions.Compiled);
                    dictionary.Add(regex, texture);
                }
                catch (ArgumentException)
                {
                    Debug.LogWarning($"正規表現が間違っています。: {regexString}");
                    continue;
                }
            }
            IconDictionary = dictionary;
        }

        internal static void FindAndBuildDictionary()
        {
            var guids = AssetDatabase.FindAssets("t:FolderIconSettingScriptableObject");
            if (guids.Length == 0)
            {
                Debug.LogWarning("FolderIconSettingScriptableObjectを見つけることができませんでした。");
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var settings = AssetDatabase.LoadAssetAtPath<FolderIconSettingScriptableObject>(path);
            BuildDictionary(settings);
        }
    }
}