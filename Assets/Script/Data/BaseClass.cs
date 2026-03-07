using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class BaseClass : ScriptableObject
{
    public string theName;
    public float baseHP;
    public float curHP;

    public float baseMP;
    public float curMP;

    public float baseATK;
    public float curATK;

    public float baseMATK;
    public float curMATK;

    public float baseDEF;
    public float curDEF;

    public float baseMDEF;
    public float curMDEF;

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
