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
    //当前场景是否存在食物 
    public static  bool isExistFood = false;


    public

    void Start()
    {
        InitData();
    }
    void InitData()
    {
        m_PosX = 940;
        m_PosY = 515;
    }

    void Update()
    {
        RandomInsFood();
    }

    public void FoodByEat()
    {
        isExistFood = false;
    }


    /// <summary>
    /// 随机生成食物
    /// </summary>
    private void RandomInsFood()
    {
        if (!isExistFood)
        { 
            GameObject foodClone = Instantiate(foodProfab, parentTrs, false);
            foodClone.GetComponent<Image>().sprite = spr_Food[Random.Range(0, spr_Food.Length)];
            foodClone.transform.localPosition = new Vector3(Random.Range(-m_PosX, m_PosX), Random.Range(-m_PosY, m_PosY), 0);
            isExistFood = true;
        }
    }
}
