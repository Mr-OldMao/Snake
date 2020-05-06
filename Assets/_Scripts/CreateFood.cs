using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 食物
/// 用于被玩家“吃掉”=》加分
/// </summary>
public class CreateFood : MonoBehaviour
{
    public GameObject foodProfab;
    public Sprite[] spr_Food;
    public Transform parentTrs;

    //食物所生成的范围 绝对值
    private float m_PosX;
    private float m_PosY;

    public static int FoodMinCount;  //场景中存在“食物”个数下限
    public static int curFoodCount;  //当前场景中存在“食物”个数



    public void Start()
    {
        InitData();
    }
    void InitData()
    {
        curFoodCount = 0;
        if (GameManager.curGameModel == GameModel.Game)
        {
            m_PosX = 1780;
            m_PosY = 880;
            FoodMinCount = 2;
        } 
        else
        {
            m_PosX = 940;
            m_PosY = 515;
            FoodMinCount = 1;
        } 
    }

    void Update()
    {
        if (curFoodCount < FoodMinCount)
            RandomInsFood();
    }


    /// <summary>
    /// 随机生成食物
    /// </summary>
    private void RandomInsFood()
    {
        GameObject foodClone = Instantiate(foodProfab, parentTrs, false);
        foodClone.GetComponent<Image>().sprite = spr_Food[Random.Range(0, spr_Food.Length)];
        foodClone.transform.localPosition = new Vector3(Random.Range(-m_PosX, m_PosX), Random.Range(-m_PosY, m_PosY), 0);
        curFoodCount++;
    }
}
