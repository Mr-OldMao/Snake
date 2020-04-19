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
 
 
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="audioClipIndex">0-BG  1-吃食物 2-死亡</param>
    public void PlayAudio(int audioClipIndex)
    { 
        if (audioClipIndex == 0)
        {
            player_BG.clip = audioClip[audioClipIndex];
            player_BG.loop = true;
            player_BG.Play();
        }
        else if (audioClipIndex == 1 || audioClipIndex == 2)
        {
            player_OtherG.clip = audioClip[audioClipIndex];
            if (!player_OtherG.isPlaying)
            {
                player_OtherG.Play();
            }
        }
        else
            Debug.LogError("数组越界 请检查 audioClipIndex = " + audioClipIndex);
    }

    /// <summary>
    /// 停止播放音效
    /// </summary>
    /// <param name="audioClipIndex">0-BG  1-吃食物 2-死亡</param>
    public void StopAudio(int audioClipIndex)
    {
        if (audioClipIndex == 0)
        {
            player_BG.clip = audioClip[audioClipIndex];
            player_BG.Stop();
        }
        else if (audioClipIndex == 1 || audioClipIndex == 2)
        {
            player_OtherG.clip = audioClip[audioClipIndex];
            player_OtherG.Stop();
        }
        else
            Debug.LogError("数组越界 请检查 audioClipIndex = " + audioClipIndex);
    }
}
