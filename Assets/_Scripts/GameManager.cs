using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class GameManager : MonoBehaviour
{
    public AudioManager audioManagerScript;
    public static GameModel curGameModel;          //接收StartScene场景下所选择的游戏模式
    public static GameSkin curGameSkin;            //接收StartScene场景下所选择的游戏皮肤



    private static GameManager m_Instance = null;
    private GameManager() { }
    public static GameManager GetInstatnce()
    {
        if (m_Instance == null)
            m_Instance = new GameManager();
        return m_Instance;
    }  


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
    /// 竞争模式
    /// </summary>
    Game
}
public enum GameSkin
{
    Yellow,
    Bule
}
