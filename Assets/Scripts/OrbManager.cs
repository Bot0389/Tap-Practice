using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;


public class OrbManager : MonoBehaviour
{


    public GameObject gameManager;

    public Sprite[] orbPicture = new Sprite[3]; //オーブの絵


    public enum ORB_KIND //オーブの種類を定義
    {

        BLUE,
        GREEN,
        PURPLE,



    };

    private ORB_KIND orbKind;

    // Use this for initialization
    void Start()
    {

        gameManager = GameObject.Find("GameManager");

    }

    // Update is called once per frame
    void Update()
    {

    }



    public void SetKind(ORB_KIND kind)
    {
        orbKind = kind;

        switch (orbKind)
        {

            case ORB_KIND.BLUE:
                GetComponent<Image>().sprite = orbPicture[0];
                break;

            case ORB_KIND.GREEN:
                GetComponent<Image>().sprite = orbPicture[1];
                break;

            case ORB_KIND.PURPLE:
                GetComponent<Image>().sprite = orbPicture[2];
                break;


        }


    }





    //オーブ取得

    public void TouchOrb()
    {

        if (Input.GetMouseButton(0) == false)
        {

            return;


        }


        RectTransform rect = GetComponent<RectTransform>();

        //オーブの軌跡
        Vector3[] path = {

            new Vector3(rect.localPosition.x * 1.5f,300f,0f),
            new Vector3(0f, 150f, 0f),

        };

        //DOTweenを使ったアニメーション
        rect.DOLocalPath(path, 0.5f, PathType.CatmullRom)
            .SetEase(Ease.OutQuad)
            .OnComplete(AddOrbPoint);

        //
        rect.DOScale(

            new Vector3 (0.5f,0.5f,0f),
            0.5f

        );



    }



    public void AddOrbPoint()
    {
        switch (orbKind)
        {

            case ORB_KIND.BLUE:
                gameManager.GetComponent<GameManager>().GetOrb(1);
                break;

            case ORB_KIND.GREEN:
                gameManager.GetComponent<GameManager>().GetOrb(5);
                break;

            case ORB_KIND.PURPLE:
                gameManager.GetComponent<GameManager>().GetOrb(10);
                break;


        }

        Destroy(this.gameObject);

    }




}
