using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class BaseClass : ScriptableObject
{
    public string theName;
    public int baseHP;
    public int curHP;

    public int baseMP;
    public int curMP;

    public int baseATK;
    public int curATK;

    public int baseMATK;
    public int curMATK;

    public int baseDEF;
    public int curDEF;

    public int baseMDEF;
    public int curMDEF;

    public List<BaseAttack> normalAttacks = new List<BaseAttack>();
    public List<BaseAttack> specialAttacks = new List<BaseAttack>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying || string.IsNullOrWhiteSpace(theName)) return;
        string assetPath = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(assetPath)) return;
        string currentFileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        if (currentFileName != theName)
        {
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                AssetDatabase.RenameAsset(assetPath, theName);
                AssetDatabase.SaveAssets();
            };
        }
    }
#endif
}
