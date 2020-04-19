using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
/// <summary>
/// 分数
/// 用于显示和更新 长度、分数、杀敌数属性
/// 更新背景颜色
/// 显示游戏结束UI、逻辑
/// </summary>
public class UIManager : MonoBehaviour
{
    public Text txt_Length;
    public Text txt_Score;
    public Text txt_Kiss;
    public Text title_Kiss;
    public Text txt_GameModelTitle;
    public Image img_BG;
    public Button btn_Enum;
    public Button btn_Parse;
    public Sprite spr_Parse;
    public Sprite spr_Play;
    public Button btn_ChangeTurn;
    public Image img_GameOver;
    public Button btn_AgainGame;
    public Button btn_ReturnStartScene;


    public SnakeHead snakeMoveScript;


    public void Start()
    {
        txt_Length.text = "1";
        txt_Score.text = "0";
        img_GameOver.gameObject.SetActive(false);
        //隐藏杀敌数UI
        if (GameManager.curGameModel != GameModel.Game)
        {
            title_Kiss.gameObject.SetActive(false);
        }
        //隐藏边框
        if (GameManager.curGameModel == GameModel.Normal)
        {
            GameObject[] borders = GameObject.FindGameObjectsWithTag("Wall");
            foreach (var item in borders)
            {
                item.GetComponent<Image>().enabled = false;
            }
        }

        //更新游戏模式
        if (GameManager.curGameModel == GameModel.Old)
        {
            txt_GameModelTitle.text = "经典模式";
        }
        else if (GameManager.curGameModel == GameModel.Normal)
        {
            txt_GameModelTitle.text = "普通模式";
        }
        else if (GameManager.curGameModel == GameModel.Game)
        {
            txt_GameModelTitle.text = "竞技模式";
        }

        btn_Enum.onClick.AddListener(() =>
        {
            //切换菜单界面
            SceneManager.LoadScene("StartScene");
        });
        btn_Parse.onClick.AddListener(() =>
        {
            //暂停 
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
                btn_Parse.GetComponent<Button>().image.sprite = spr_Play;
            }
            //播放
            else if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
                btn_Parse.GetComponent<Button>().image.sprite = spr_Parse;
            }
        });
        //切换旋转方式
        btn_ChangeTurn.onClick.AddListener(() =>
        {
            snakeMoveScript.ChangeTurnType();
        });
        //重启游戏
        btn_AgainGame.onClick.AddListener(() =>
        {
            //TODO
            txt_Length.text = "1";
            txt_Score.text = "0";
            img_GameOver.GetComponent<Animator>().SetTrigger("isGameOver");
            img_GameOver.GetComponent<Animator>().SetTrigger("isRestartGame");
            StartCoroutine("WaitHide");
            //img_GameOver.gameObject.SetActive(false); 
            snakeMoveScript.ReStartGame();
            GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayAudio(0);

        });
        //返回主菜单
        btn_ReturnStartScene.onClick.AddListener(() =>
        {
            //切换菜单界面
            SceneManager.LoadScene("StartScene");
            img_GameOver.gameObject.SetActive(false);
            Time.timeScale = 1;
        });
    }

    public void LateUpdate()
    {
        //更新UI数据
        if (txt_Length.text != SnakeHead.curSnakeLength.ToString())
        {
            txt_Length.text = SnakeHead.curSnakeLength.ToString();
            //默认增加【1，5】分
            txt_Score.text = (int.Parse(txt_Score.text) + Random.Range(1, 6)).ToString();
        }
        UpdateBGColor();
    }

    /// <summary>
    /// 更新背景颜色
    /// 根据节点数 更新指定颜色bg  
    /// </summary>
    private void UpdateBGColor()
    {
        if (SnakeHead.curSnakeLength == 1) return;
        int colorIndex = SnakeHead.curSnakeLength * 20;
        img_BG.color = Color.HSVToRGB((colorIndex / 360f) % 1, 40.0f / 256f, 1f);
    }
    public void ResetBGColor()
    {
        img_BG.color = Color.HSVToRGB(0, 0, 1);
    }

    /// <summary>
    /// 显示游戏结束UI
    /// </summary>
    public void ShowGameOver()
    {
        img_GameOver.gameObject.SetActive(true);
        img_GameOver.GetComponent<Animator>().SetTrigger("isGameOver");
        //最高分
        //写入数据 数据持久化 
        StartScene.WriteDataToRegister(SnakeHead.curSnakeLength, int.Parse(txt_Score.text), 0);
    }

    /// <summary>
    /// 延时隐藏GameOver面板
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitHide()
    {
        yield return new WaitForSeconds(0.3f);
        img_GameOver.gameObject.SetActive(false);
        img_GameOver.transform.localPosition = new Vector3(0, 873, 0);
    }
}
