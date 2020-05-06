using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 配置小地图
/// </summary>
public class MiniMapSet : MonoBehaviour
{
    public Image img_PlayerIndicator;
    public Transform playerRotation;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //小地图玩家图标正方向
        img_PlayerIndicator.transform.localRotation = playerRotation.localRotation;
        img_PlayerIndicator.transform.localEulerAngles = new Vector3(0, 90, 0);
        Debug.Log("旋转   小地图：" + img_PlayerIndicator.transform.localRotation + ",蛇头：" + playerRotation.localRotation);
       
    }
}
