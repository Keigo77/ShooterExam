using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRChatFolderIcons.Editor 
{
    public class FolderIconSettingScriptableObject : ScriptableObject {
        
        public bool enable = true;
        public List<FolderIconSetting> folderIconSettings = new List<FolderIconSetting>();
        
        public void OnValidate() {
            IconDictionaryCreator.BuildDictionary(this);
        }
    }
    
    [System.Serializable]
    public class FolderIconSetting {
        public bool enable = true;
        public Texture2D icon;
        public string regex;
        public MatchType matchType;
    }

    public enum MatchType {
        FullMatch,
        PartialMatch
    }
}

