using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
/// <summary>
/// 分数
/// 用于显示和更新 长度、分数、杀敌数属性
/// 更新背景颜色
/// 显示游戏结束UI、逻辑
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("左上角核心数显UI")]
    public Text txt_Length;
    public Text txt_Score;
    public Text txt_Kiss;
    public Text title_KissContainer;
    [Header("场景UI")]
    [Space(10)]
    public Text txt_GameModelTitle;
    public Image[] img_BG;
    public GameObject[] img_Boder;
    [Header("右侧功能UI")]
    [Space(10)]
    public Button btn_Enum;
    public Button btn_Parse;
    public Sprite spr_Parse;
    public Sprite spr_Play;
    public Button btn_ChangeTurn;
    [Header("加速UI")]
    [Space(10)]
    public Button btn_SpeedUp;
    public Button btn_SpeedUpContinue;
    public Image img_PassOnAnim;
    [Header("游戏结束UI（玩家输） ")]
    [Space(10)]
    public Image img_GameOver;
    public Button btn_AgainGame;
    public Button btn_ReturnStartScene;
    [Header("获胜UI（玩家赢）")]
    [Space(10)]
    public Image img_Win;
    public Button btn_Next;
    public Button btn_BackStartScene;
    [Header("小地图UI")]
    [Space(10)]
    public Canvas cvs_MiniMap;
    public Button btn_MiniMapAdd;
    public Button btn_MiniMapSub;
    [Header("音量调节UI")]
    [Space(10)]
    public Button btn_Audio;
    public Image img_AudioVolume;    //音量控制画布
    public Button btn_ExitAudioVolume;
    public Image img_BGAudioIcon;     //背景音乐图标
    public Image img_SoundAudioIcon;  //音效图标
    public Sprite[] spr_AudioIcon;     //0 低音量图标 1-中音量图标 2-高音量图标
    public Slider sli_BGAudio;
    public Slider sli_SoundAudio;

    [Space(10)]
    public SnakeHead snakeMoveScript;
    private AudioManager m_AudiomanagerScript;

    private static UIManager m_Instance;
    public static UIManager GetInstance
    {
        get { return m_Instance; }
    }
    void Awake()
    {
        m_Instance = this;
        m_AudiomanagerScript = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    public void Start()
    {
        #region 初始化数据 
        txt_Length.text = "1";
        txt_Score.text = "0";
        txt_Kiss.text = "0";
        img_GameOver.gameObject.SetActive(false);
        img_AudioVolume.gameObject.SetActive(false);
        img_Win.gameObject.SetActive(false);

        //隐藏杀敌数UI
        if (GameManager.curGameModel != GameModel.Game)
        {
            title_KissContainer.gameObject.SetActive(false);
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

        //根据游戏的模式  更新UI模式  
        SetUIBySelectModel();

        if (GameManager.curGameModel != GameModel.Game)
        {
            GameObject.Find("Canvas_Enemy").SetActive(false);
        }
        #endregion

        #region 右上角功能按钮 
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
                btn_Parse.GetComponent<Button>().image.color = Color.red;
            }
            //播放
            else if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
                btn_Parse.GetComponent<Button>().image.sprite = spr_Parse;
                btn_Parse.GetComponent<Button>().image.color = Color.black;
            }
        });

        //切换旋转方式
        btn_ChangeTurn.onClick.AddListener(() =>
        {
            snakeMoveScript.ChangeTurnType();
        });
        #endregion

        #region 重启游戏逻辑 
        //重启游戏
        btn_AgainGame.onClick.AddListener(() =>
        {
            //TODO
            txt_Length.text = "1";
            txt_Score.text = "0";
            txt_Kiss.text = "0";
            img_GameOver.GetComponent<Animator>().SetTrigger("isGameOver");
            img_GameOver.GetComponent<Animator>().SetTrigger("isRestartGame");
            StartCoroutine("WaitHide");
            snakeMoveScript.ReStartGame();
            GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayAudio(0);
            if (GameManager.curGameModel == GameModel.Game)
                GameObject.FindGameObjectWithTag("EnemyHead").GetComponent<EnemyAI>().ReStartGame();
        });
        //返回主菜单
        btn_ReturnStartScene.onClick.AddListener(() =>
        {
            //切换菜单界面
            SceneManager.LoadScene("StartScene");
            img_GameOver.gameObject.SetActive(false);
            Time.timeScale = 1;
        });
        #endregion

        #region 下一关卡 
        btn_Next.onClick.AddListener(() =>
        {
            //TODO
            txt_Length.text = "1";
            txt_Score.text = "0"; 
            img_Win.gameObject.SetActive(false);
            snakeMoveScript.ReStartGame();
            GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayAudio(0);
            if (GameManager.curGameModel == GameModel.Game)
            {
                //GameObject.FindGameObjectWithTag("EnemyHead").GetComponent<EnemyAI>().PlayerWinNextLevel();
                GameObject.FindGameObjectWithTag("EnemyHead").GetComponent<EnemyAI>().ReStartGame();
            } 
        });
        btn_BackStartScene.onClick.AddListener(() =>
        {
            //写入数据 数据持久化 
            StartScene.WriteDataToRegister(SnakeHead.curSnakeLength, int.Parse(txt_Score.text), int.Parse(txt_Kiss.text));
            //切换菜单界面
            SceneManager.LoadScene("StartScene");
            img_Win.gameObject.SetActive(false);
            Time.timeScale = 1;
        });
        #endregion

        #region 小地图交互 
        btn_MiniMapAdd.onClick.AddListener(() =>
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ExampleInteractions>().MiniMapInteraction(true);
        });
        btn_MiniMapSub.onClick.AddListener(() =>
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ExampleInteractions>().MiniMapInteraction(false);
        });
        #endregion

        #region 音量调节事件
        //打开/关闭 画布
        btn_Audio.onClick.AddListener(() =>
        {
            if (img_AudioVolume.IsActive())
                img_AudioVolume.gameObject.SetActive(false);
            else
                img_AudioVolume.gameObject.SetActive(true);
        });
        //关闭画布
        btn_ExitAudioVolume.onClick.AddListener(() =>
        {
            if (img_AudioVolume.IsActive())
                img_AudioVolume.gameObject.SetActive(false);
        });

        #endregion
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

        //背景音乐 滑动条
        if (img_AudioVolume.IsActive() && m_AudiomanagerScript)
        {
            m_AudiomanagerScript.ChangeVolume(0, sli_BGAudio.value);
            m_AudiomanagerScript.ChangeVolume(1, sli_SoundAudio.value);
            ChangeVolumeIcon();
        }
    }

    /// <summary>
    /// 更新背景颜色
    /// 根据节点数 更新指定颜色bg  
    /// </summary>
    private void UpdateBGColor()
    {
        if (SnakeHead.curSnakeLength == 1) return;
        int colorIndex = SnakeHead.curSnakeLength * 20;
        if (GameManager.curGameModel != GameModel.Game) img_BG[0].color = Color.HSVToRGB((colorIndex / 360f) % 1, 40.0f / 256f, 1f);
        else img_BG[1].color = Color.HSVToRGB((colorIndex / 360f) % 1, 40.0f / 256f, 1f);

    }
    public void ResetBGColor()
    {
        if (GameManager.curGameModel != GameModel.Game)
            img_BG[0].color = Color.HSVToRGB(0, 0, 1);
        else
            img_BG[1].color = Color.HSVToRGB(0, 0, 1);
    }

    /// <summary>
    /// 显示游戏结束UI
    /// </summary>
    public void ShowGameOver()
    {
        img_GameOver.gameObject.SetActive(true);
        if (GameManager.curGameModel != GameModel.Game)
        {
            img_GameOver.GetComponent<Animator>().enabled = true;
            img_GameOver.GetComponent<Animator>().SetTrigger("isGameOver");
        }
        else
        {
            img_GameOver.GetComponent<Animator>().enabled = false;
            img_GameOver.transform.localPosition = new Vector3(snakeMoveScript.transform.localPosition.x, snakeMoveScript.transform.localPosition.y, 0);
        }
        //写入数据 数据持久化 
        StartScene.WriteDataToRegister(SnakeHead.curSnakeLength, int.Parse(txt_Score.text), int.Parse(txt_Kiss.text));
    }

    /// <summary>
    /// 延时隐藏GameOver面板
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitHide()
    {
        if (GameManager.curGameModel != GameModel.Game)
            yield return new WaitForSeconds(0.3f);
        img_GameOver.gameObject.SetActive(false);
        img_GameOver.transform.localPosition = new Vector3(0, 873, 0);
    }

    /// <summary>
    /// 显示 btn_SpeedUp
    /// </summary>
    public void DisplaySpeedUpBtn(bool isShow)
    {
        btn_SpeedUp.gameObject.SetActive(isShow);
        btn_SpeedUpContinue.gameObject.SetActive(false);
        img_PassOnAnim.gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示“获胜”画布
    /// </summary>
    public void DisplayWinImg()
    {
        img_Win.gameObject.SetActive(true);
        txt_Kiss.text = (int.Parse(txt_Kiss.text) + 1).ToString();
    }

    /// <summary>
    ///  根据选择的模式 设置响应UI
    /// </summary>
    /// <param name="isGameModel"></param>
    private void SetUIBySelectModel()
    {
        switch (GameManager.curGameModel)
        {
            case GameModel.Old:
                txt_GameModelTitle.text = "经典模式";
                //背景图
                img_BG[0].gameObject.SetActive(true);
                img_BG[1].gameObject.SetActive(false);
                //边界
                img_Boder[0].gameObject.SetActive(true);
                img_Boder[1].gameObject.SetActive(false);
                //小地图
                cvs_MiniMap.gameObject.SetActive(false);
                break;
            case GameModel.Normal:
                txt_GameModelTitle.text = "普通模式";
                img_BG[0].gameObject.SetActive(true);
                img_BG[1].gameObject.SetActive(false);
                img_Boder[0].gameObject.SetActive(true);
                img_Boder[1].gameObject.SetActive(false);
                cvs_MiniMap.gameObject.SetActive(false);
                break;
            case GameModel.Game:
                txt_GameModelTitle.text = "竞技模式";
                img_BG[0].gameObject.SetActive(false);
                img_BG[1].gameObject.SetActive(true);
                img_Boder[0].gameObject.SetActive(false);
                img_Boder[1].gameObject.SetActive(true);
                cvs_MiniMap.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 改变音量图标
    /// </summary>
    private void ChangeVolumeIcon()
    {
        //背景音乐图标
        if (sli_BGAudio.value == 0)
        {
            img_BGAudioIcon.sprite = spr_AudioIcon[0];
        }
        else if (sli_BGAudio.value > 0 && sli_BGAudio.value <= 0.3)
        {
            img_BGAudioIcon.sprite = spr_AudioIcon[1];

        }
        else if (sli_BGAudio.value > 0.3 && sli_BGAudio.value < 0.8)
        {
            img_BGAudioIcon.sprite = spr_AudioIcon[2];
        }
        else if (sli_BGAudio.value >= 0.8)
        {
            img_BGAudioIcon.sprite = spr_AudioIcon[3];
        }
        //音效图标
        if (sli_SoundAudio.value == 0)
        {
            img_SoundAudioIcon.sprite = spr_AudioIcon[0];
        }
        else if (sli_SoundAudio.value > 0 && sli_SoundAudio.value <= 0.3)
        {
            img_SoundAudioIcon.sprite = spr_AudioIcon[1];
        }
        else if (sli_SoundAudio.value > 0.3 && sli_SoundAudio.value < 0.8)
        {
            img_SoundAudioIcon.sprite = spr_AudioIcon[2];
        }
        else if (sli_SoundAudio.value >= 0.8)
        {
            img_SoundAudioIcon.sprite = spr_AudioIcon[3];
        }
    }

}
