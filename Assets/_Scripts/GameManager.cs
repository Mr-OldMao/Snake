using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class GameManager : MonoBehaviour
{
    public AudioManager audioManagerScript;
    void Start()
    {
        audioManagerScript.PlayAudio(0);
    }
}
/// <summary>
/// 打包的类型
/// </summary>
public enum BuildType
{
    PC,
    Android
}
public enum GameModel
{
    /// <summary>
    /// 经典模式
    /// </summary>
    Old,
    /// <summary>
    /// 普通模式
    /// </summary>
    Normal,
    /// <summary>
    /// 无尽模式
    /// </summary>
    Endless
}
