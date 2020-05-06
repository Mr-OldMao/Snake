using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioSource player_BG;   //背景音乐
    public AudioSource player_Sound;//音效
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
            player_Sound.clip = audioClip[audioClipIndex];
            if (!player_Sound.isPlaying)
            {
                player_Sound.Play();
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
            player_Sound.clip = audioClip[audioClipIndex];
            player_Sound.Stop();
        }
        else
            Debug.LogError("数组越界 请检查 audioClipIndex = " + audioClipIndex);
    }

    /// <summary>
    /// 调整音量
    /// </summary>
    /// <param name="playerIndex">0-背景播放器 1-音效播放器</param>
    /// <param name="volume">音量大小 【0，1】</param>
    public void ChangeVolume(int playerIndex,float volume)
    {
        if (playerIndex !=0 && playerIndex!=1)
        {
            Debug.LogError("playerIndex 越界：" + playerIndex);
            return;
        }
        if (volume <0 || volume>1)
        {
            Debug.LogError("volume 越界：" + volume);
            return;
        }
        if (playerIndex == 0)
        {
            player_BG.volume = volume;
        }
        else  if (playerIndex == 1)
        {
            player_Sound.volume = volume;
        }
    }
}
