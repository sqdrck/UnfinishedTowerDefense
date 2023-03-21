using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "ResourcePack", menuName = "ScriptableObjects/Resource Pack", order = 1)]
public class ResourcePackSo : SerializedScriptableObject
{
    public ResourcePack Value;
}
