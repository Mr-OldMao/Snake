using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Game模式摄像机
/// </summary>
public class GameCamera : MonoBehaviour
{
    public Transform playerSnakeHeadPos;
    void Update()
    {
        if (GameManager.curGameModel == GameModel.Game)
        { 
            transform.position = new Vector3(playerSnakeHeadPos.position.x, playerSnakeHeadPos.position.y,-10);
        }
    }
}
