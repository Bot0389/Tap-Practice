using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class GameManager : MonoBehaviour
{

    //定数定義
    private const int MAX_ORB = 10;//オーブ最大値
    private const int RESPAWN_TIME = 5; //オーブが発生する秒数
    private const int MAX_LEVEL = 2; //最大お寺レベル

    //データセーブ用Key
    private const string KEY_SCORE = "SCORE"; //スコア
    private const string KEY_LEVEL = "LEVEL"; //レベル
    private const string KEY_ORB = "ORB";     //オーブ数
    private const string KEY_TIME = "TIME";   //時間



    //オブジェクト参照
    public GameObject orbPrefab;  //オーブプレハブ
    public GameObject smokePrefab;//煙プレハブ
    public GameObject kusudamaPrefab;//くす玉プレハブ
    public GameObject canvasGame; //ゲームキャンバス
    public GameObject textscore; //スコアテキスト
    public GameObject imageTemple;//お寺
    public GameObject imageMokugyo;//木魚

    public AudioClip getScoreSE; //効果音スコアゲット
    public AudioClip levelUpSE;　//効果音レベルアップ
    public AudioClip clearSE;  //効果音クリア



    //メンバ変数
    private int currentscore = 0; //現在のスコア
    private int nextscore = 10; //レベル
    private int currentOrb = 0;    //現在のオーブ
    private int templeLevel = 0;//寺のレベル
    private DateTime LastDateTime; //最後にオーブを作成した時間

    private int[] nextScoreTable = new int[] { 10, 100, 1000 };

    private AudioSource audioSource;//オーディオソース






    // Use this for initialization
    void Start()
    {
        audioSource = this.gameObject.GetComponent<AudioSource>();
        currentOrb = MAX_ORB;
        //初期設定
        currentscore = PlayerPrefs.GetInt(KEY_SCORE,0);
        templeLevel = PlayerPrefs.GetInt(KEY_LEVEL, 0);
        currentOrb = PlayerPrefs.GetInt(KEY_ORB,10);


        //初期オーブ作成
        for (int i = 0; i < currentOrb; i++)
        {
            CreateOrb();
        }

        //時間の復元
        string time = PlayerPrefs.GetString(KEY_TIME,"");
        if (time == "")
        {
            //時間がセーブされてない場合現在時間
            LastDateTime = DateTime.UtcNow;
        }
        else {
            long temp = Convert.ToInt64(time);
            LastDateTime = DateTime.FromBinary(temp);


        }



        nextscore = nextScoreTable[templeLevel];
        imageTemple.GetComponent<TempleManager>().SetTemplePicture(templeLevel);
        imageTemple.GetComponent<TempleManager>().SetTempleScale(currentscore, nextscore);

        RefreshScoreText();



    }

    // Update is called once per frame
    void Update()
    {

        if (currentOrb < MAX_ORB)
        {
            TimeSpan timeSpan = DateTime.UtcNow - LastDateTime;

            if (timeSpan >= TimeSpan.FromSeconds(RESPAWN_TIME))
            {

                while (timeSpan >= TimeSpan.FromSeconds(RESPAWN_TIME))
                {


                    CreateNewOrb();
                    timeSpan -= TimeSpan.FromSeconds(RESPAWN_TIME);

                    CreateNewOrb();
                    timeSpan -= TimeSpan.FromSeconds(RESPAWN_TIME);
                }

            }

        }

    }


    //寺のレベル管理
    void TempleLevelUp()
    {

        if (currentscore>=nextscore)
        {
            if (templeLevel<MAX_LEVEL)
            {

                templeLevel++;

                currentscore = 0;

                TempleLevelUpEffect();


                nextscore = nextScoreTable[templeLevel];

                imageTemple.GetComponent<TempleManager>().SetTemplePicture(templeLevel);


            }

        }

    }



    //レベルアップ時の演出
    void TempleLevelUpEffect()
    {
        GameObject smoke = (GameObject)Instantiate(smokePrefab);
        smoke.transform.SetParent(canvasGame.transform, false);
        smoke.transform.SetSiblingIndex(2);

        audioSource.PlayOneShot(levelUpSE);

        Destroy(smoke,0.5f);

    }


   


    //新しいオーブの作成
    public void CreateNewOrb()
    {

        LastDateTime = DateTime.UtcNow;

        //最大数だったら戻るよ
        if (currentOrb >= MAX_ORB)
        {

            return;

        }

        CreateOrb();
        //増やすよ
        currentOrb++;

        SaveGameData();

    }



    //オーブ生成
    public void CreateOrb()
    {

        GameObject orb = Instantiate(orbPrefab);

        //transform.parentとSetParentの挙動
        //GameObjectで親子関係を作るには、gameObjXXX.transform.parent に親のtransformを入れるだけ
        //だけど、そうすると、ワールド空間上での相対的な位置を引き継ぐという余計な機能が働く（エディタ的には素晴らしい機能だけどスクリプト的には余計です）
        //Unity4.6から、SetParent(parentGameObj, false)とやるとその機能をKill出来るそうです。


        orb.transform.SetParent(canvasGame.transform, false);


        //ポジションの設定
        orb.transform.localPosition = new Vector3(
            UnityEngine.Random.Range(-300.0f, 300.0f),
            UnityEngine.Random.Range(-180.0f, 180.0f),
            0f
         );


        //オーブの種類を設定
        int kind = UnityEngine.Random.Range(0, templeLevel + 1);
        {
            switch (kind)
            {

                case 0 :
                    orb.GetComponent<OrbManager>().SetKind(OrbManager.ORB_KIND.BLUE);
                    break;
                case 1:
                    orb.GetComponent<OrbManager>().SetKind(OrbManager.ORB_KIND.GREEN);
                    break;
                case 2:
                    orb.GetComponent<OrbManager>().SetKind(OrbManager.ORB_KIND.PURPLE);
                    break;


            }




        }








    }

    //オーブの入手

    public void GetOrb(int getScore)
    {
        audioSource.PlayOneShot(getScoreSE);

        AnimatorStateInfo stateInfo =
            imageMokugyo.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        if (stateInfo.fullPathHash == Animator.StringToHash("Base Layer.get@Imagemokugyo"))
        {
            //すでに再生中なら先頭から
            imageMokugyo.GetComponent<Animator>().Play(stateInfo.fullPathHash, 0, 0.0f);

        }
        else
        {
            imageMokugyo.GetComponent<Animator>().SetTrigger("isGetScore");
        }


        if (currentscore < nextscore)
        {
            currentscore += getScore;

            currentscore += 1;

            //レベルアップの値を超えないように制限
            if (currentscore > nextscore)
            {
                currentscore = nextscore;

            }

            TempleLevelUp();
            RefreshScoreText();

            imageTemple.GetComponent<TempleManager>().SetTempleScale(currentscore, nextscore);

            //ゲームクリア判定
            if ((currentscore == nextscore) && (templeLevel == MAX_LEVEL))
            {
                ClearEffect();

            }


            //減らすよ
            currentOrb--;

            SaveGameData();
        }

        }

    void RefreshScoreText()
    {
        textscore.GetComponent<Text>().text = "徳:" + currentscore + "/" + nextscore;

    }

 //クリア演出
    void ClearEffect()
    {
        GameObject kusudama = (GameObject)Instantiate(kusudamaPrefab);
        kusudama.transform.SetParent(canvasGame.transform,false);


        audioSource.PlayOneShot(clearSE);

    }

    void SaveGameData() {

        PlayerPrefs.SetInt(KEY_SCORE,currentscore);
        PlayerPrefs.SetInt(KEY_LEVEL, templeLevel);
        PlayerPrefs.SetInt(KEY_ORB,currentOrb);
        PlayerPrefs.SetString(KEY_TIME,LastDateTime.ToBinary().ToString());

        PlayerPrefs.Save();
    }



}