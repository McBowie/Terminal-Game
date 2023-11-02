using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "File", menuName = "ScriptableObjects/ScriptableFile", order = 1)]
public class ScriptableFile : ScriptableObject
{
    [SerializeField] public string myName, extension, path;
    [SerializeField][TextArea(15, 20)] public string content;

}
