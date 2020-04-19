using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
/// <summary>
/// 开始菜单场景
/// </summary>
public class StartScene : MonoBehaviour
{
    public Button btn_StartGame;
    public Button btn_ExitGame;
    public Button btn_Rule;                     //控制面板规则开关
    public Button btn_ExitRule;               //关闭规则面板
    public Toggle[] tge_Model;                //游戏模式
    public Toggle[] tge_Skin;                 //皮肤  0-小蓝 1-小黄 
    public Text[] txt_Bast;                  //最高的 0-长度 1-分数 2-杀敌数
    public Text[] txt_Before;                //上一次的 0-长度 1-分数 2-杀敌数  
    public Image img_Rule;                   //规则面板
    void Awake()
    {
        //设置分辨率
        Screen.SetResolution(1920, 1080, false);
    }

    // Use this for initialization
    void Start()
    {
        //DeleteLogin()  测试辅助工具
        img_Rule.gameObject.SetActive(false);
        btn_StartGame.onClick.AddListener(() =>
        {
            //向Main场景传输所选的属性
            //游戏模式
            if (tge_Model[0].isOn)
            {
                GameManager.curGameModel = GameModel.Old;
            }
            else if (tge_Model[1].isOn)
            {
                GameManager.curGameModel = GameModel.Normal;

                Debug.Log(GameManager.curGameModel);
            }
            else if (tge_Model[2].isOn)
            {
                GameManager.curGameModel = GameModel.Game;
            }
            //皮肤
            if (tge_Skin[0].isOn)
            {
                GameManager.curGameSkin = GameSkin.Bule;
            }
            else if (tge_Skin[1].isOn)
            {
                GameManager.curGameSkin = GameSkin.Yellow;
            }
            CreateFood.isExistFood = false;
            SceneManager.LoadScene("Main");
        });
        //开关规则img
        btn_ExitRule.onClick.AddListener(() =>
        {
            img_Rule.GetComponent<Animator>().SetTrigger("exit");
            StartCoroutine("WaitAnimClose");
        });
        btn_Rule.onClick.AddListener(() =>
        {
            RulePanleAnim();
        });

        btn_ExitGame.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        //更新成绩
        UpdateScore();
    }

    private void RulePanleAnim()
    {
        if (!img_Rule.gameObject.activeSelf)
        {
            img_Rule.gameObject.SetActive(true);
            img_Rule.GetComponent<Animator>().SetTrigger("open");
        }
        else
        {
            img_Rule.GetComponent<Animator>().SetTrigger("exit");
            StartCoroutine("WaitAnimClose");
        }
    }
    IEnumerator WaitAnimClose()
    {
        yield return new WaitForSeconds(0.5f);
        img_Rule.gameObject.SetActive(false);
    }



    private void UpdateScore()
    {
        //更新上一次成绩
        if (txt_Before[0].text != PlayerPrefs.GetInt("beforeLength", 0).ToString())
        {
            txt_Before[0].text = PlayerPrefs.GetInt("beforeLength", 0).ToString();
        }
        if (txt_Before[1].text != PlayerPrefs.GetInt("beforeScore", 0).ToString())
        {
            txt_Before[1].text = PlayerPrefs.GetInt("beforeScore", 0).ToString();
        }
        if (txt_Before[2].text != PlayerPrefs.GetInt("beforeKill", 0).ToString())
        {
            txt_Before[2].text = PlayerPrefs.GetInt("beforeKill", 0).ToString();
        }
        //更新最高成绩
        if (txt_Bast[0].text != PlayerPrefs.GetInt("bestLength", 0).ToString())
        {
            txt_Bast[0].text = PlayerPrefs.GetInt("bestLength", 0).ToString();
        }
        if (txt_Bast[1].text != PlayerPrefs.GetInt("bestScore", 0).ToString())
        {
            txt_Bast[1].text = PlayerPrefs.GetInt("bestScore", 0).ToString();
        }
        if (txt_Bast[2].text != PlayerPrefs.GetInt("bestKill", 0).ToString())
        {
            txt_Bast[2].text = PlayerPrefs.GetInt("bestKill", 0).ToString();
        }
    }

    /// <summary>
    /// 清理注册表
    /// </summary>
    private void DeleteLogin()
    {
        PlayerPrefs.DeleteAll();
    }

    /// <summary>
    /// 写入数据到注册表  
    /// 写入当前数据和更新最高记录数据
    /// </summary>
    /// <param name="curData">本轮成绩 0-长度 1-分数 2-杀敌数</param>
    public static void WriteDataToRegister(int length, int score, int killNum)
    {
        //数据持久化 写入数据
        //上一次数据
        PlayerPrefs.SetInt("beforeLength", length);
        PlayerPrefs.SetInt("beforeScore", score);
        PlayerPrefs.SetInt("beforeKill", killNum);
        // 判定最高记录 
        if (length >= PlayerPrefs.GetInt("bestLength", 0) &&
         score >= PlayerPrefs.GetInt("bestScore", 0) &&
       killNum >= PlayerPrefs.GetInt("bestKill", 0))
        {
            PlayerPrefs.SetInt("bestLength", length);
            PlayerPrefs.SetInt("bestScore", score);
            PlayerPrefs.SetInt("bestKill", killNum);
        }
    }
}
