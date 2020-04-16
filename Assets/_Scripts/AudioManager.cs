using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioSource player_BG;
    public AudioSource player_OtherG;
    /// <summary>
    /// 0-BG  1-吃食物 2-死亡
    /// </summary>
    public AudioClip[] audioClip;

    //private static AudioManager m_Instace = null;
    //public static AudioManager GetInstance
    //{
    //    get
    //    {
    //        if (m_Instace == null)
    //            m_Instace = new AudioManager();
    //        return m_Instace;
    //    }
    //}




    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="audioClipIndex">0-BG  1-吃食物 2-死亡</param>
    public void PlayAudio(int audioClipIndex)
    { 
        if (audioClipIndex == 0)
        {
            player_BG.clip = audioClip[audioClipIndex];
            player_BG.Play();
        }
        else if (audioClipIndex == 1 || audioClipIndex == 2)
        {
            player_OtherG.clip = audioClip[audioClipIndex];
            player_OtherG.Play();
        }
        else
            Debug.LogError("数组越界 请检查 audioClipIndex = " + audioClipIndex);
    }
}
