using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class UIController : MonoBehaviour
{
    public int myOrder = -1;
    public int lastDiscardedTileNum = -1;
    public List<Person> people;
    public bool notActive = false;
    public bool isClick = false;
    public HandContainer myHandContainer;   //手牌の親オブジェクト.
    public GameManager gameManager;
    public Checker checker;     //役判定スクリプト.
    public TileController tileController;
    public SoundManager soundManager;
    public GameObject[] handOfWinner;
    public GameObject[] kongOfWinner;

    public enum BUTTON_TYPE
    {
        NONE = -1,
        DRAW = 0,
        RON,
        MELDED_KONG,
        CONCEALED_KONG,
        NUM,
    };
    public BUTTON_TYPE buttonType = BUTTON_TYPE.NONE;

    public enum YAKU_TYPE
    {
        NONE = -1,
        BLESSING_OF_HEAVEN = 0,     //天和.
        BLESSING_OF_EARTH,          //地和.
        FOUR_QUADS,                 //四槓子.
        COUNTING_GRAND_SLUM,        //数え役満.
        FLOWING_YAKUMANN,           //流れ役満.
        NUM,
    };
    public YAKU_TYPE yakuType = YAKU_TYPE.NONE;

    [Header("Dora Panel")]
    public GameObject Dora_Panel;
    public GameObject fingerImage;
    public GameObject swipeText;
    public GameObject waitText;

    [Header("Playing Panel")]
    public GameObject Playing_Panel;

    [Header("Main Play Panel Info")]
    public TextMeshProUGUI wallTilesNum;
    public TextMeshProUGUI personName0;
    public TextMeshProUGUI personName1;
    public TextMeshProUGUI personName2;
    public TextMeshProUGUI personName3;
    public List<TextMeshProUGUI> peopleNames;
    public GameObject furitennImage;
    public GameObject cantDiscardText;

    [Header("Point Panel Info")]
    public TextMeshProUGUI kongPt;
    public TextMeshProUGUI meldedKongPt;
    public TextMeshProUGUI doraPt;
    public TextMeshProUGUI kingsTilePt;
    public TextMeshProUGUI finalTilePt;
    public TextMeshProUGUI totalPt;

    [Header("Button Panel Info")]
    public GameObject ronButton;
    public GameObject drawButton;
    public GameObject meldedKongButton;     //大明槓ボタン.
    public GameObject concealedKongButton;  //暗槓ボタン.
    public GameObject skipButton;
    public GameObject selectConcealedKongTile;
    public GameObject tileImage0;   //暗槓できる牌が2つの時の1つ目.
    public GameObject tileImage1;   //暗槓できる牌が2つの時の2つ目.

    public GameObject leaveCheck;
    public GameObject ckeckText;
    public GameObject yesButton;
    public GameObject noButton;
    public GameObject reportTextObject;
    public TextMeshProUGUI reportText;

    [Header("Effect Panel Info")]
    public GameObject ronEffect0;   //自分のロンエフェクト.
    public GameObject ronEffect1;   //右側の人のロンエフェクト.
    public GameObject ronEffect2;   //対面の人のロンエフェクト.
    public GameObject ronEffect3;   //左側の人のロンエフェクト.
    public List<GameObject> ronEffects;
    public GameObject drawEffect0;
    public GameObject drawEffect1;
    public GameObject drawEffect2;
    public GameObject drawEffect3;
    public List<GameObject> drawEffects;

    public GameObject concealedKongEffect0;
    public GameObject concealedKongEffect1;
    public GameObject concealedKongEffect2;
    public GameObject concealedKongEffect3;
    public List<GameObject> concealedKongEffects;
    public GameObject meldedKongEffect0;
    public GameObject meldedKongEffect1;
    public GameObject meldedKongEffect2;
    public GameObject meldedKongEffect3;
    public List<GameObject> meldedKongEffects;

    [Header("Result Panel")]
    public GameObject Result_Panel;
    public TextMeshProUGUI winnerName;
    public GameObject BlessingOfHeaven_Panel;
    public GameObject BlessingOfEarth_Panel;
    public GameObject FourQuads_Panel;
    public GameObject CountingGrandSlum_Panel;
    public GameObject FlowingYakumann_Panel;
    public GameObject yakumanImage;

    [Header("Counting Grand Slum Panel Info")]
    public GameObject kongImage;
    public GameObject meldedKongImage;
    public GameObject doraImage;
    public GameObject kingsTileImage;
    public GameObject finalTileImage;
    public GameObject totalImage;

    public TextMeshProUGUI yakuOfKong;
    public TextMeshProUGUI yakuOfMeldedKong;
    public TextMeshProUGUI yakuOfDora;
    public TextMeshProUGUI yakuOfKingsTile;
    public TextMeshProUGUI yakuOfFinalTile;
    public TextMeshProUGUI yakuOfTotal;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUpInitialUI(int myOrder, List<Person> people, int wallTilesCount)
    {
        this.myOrder = myOrder;
        this.people = people;
        peopleNames = new List<TextMeshProUGUI> { personName0, personName1, personName2, personName3 };
        for (int i = 0; i < peopleNames.Count; i++)
        {
            peopleNames[i].text = people[(myOrder + i) % people.Count].Name;
        }
        UpdateUI(wallTilesCount);
        NotActiveSetUp();

        GameObject container = null;
        switch (myOrder)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                break;
            default:
                break;
        }
        myHandContainer = container.GetComponent<HandContainer>();
        gameManager = GetComponent<GameManager>();
        checker = GetComponent<Checker>();
        tileController = GetComponent<TileController>();
        soundManager = GetComponent<SoundManager>();

        /*
         * ドラ表示UIの設定.
         * ドラ表示されていない部分(初期14枚)を枝豆で表示する.
        */
        int numDoraIndicators = tileController.doraIndicators.Length;
        int initialDoraIndicatorNum = tileController.texturesOfDoraIndicator.Length - 1;
        for (int i = 0; i < numDoraIndicators; i++)
        {
            changeDoraIndicator(i, initialDoraIndicatorNum);
        }
    }


    public void UpdateUI(int wallTilesCount)
    {
        wallTilesNum.text = "牌山" + "\n" + "残：" + wallTilesCount + "枚";
        kongPt.text = "槓：" + (people[myOrder].kongCount - 1) + "翻";
        meldedKongPt.text = "大明槓：" + people[myOrder].meldedKongCount + "翻";
        doraPt.text = "ドラ：" + people[myOrder].doraCount + "翻";
        kingsTilePt.text = "嶺上開花：" + people[myOrder].kingsTileDraw + "翻";
        finalTilePt.text = "海底撈月：" + people[myOrder].finalTileWin + "翻";
        int totalPoint = (people[myOrder].kongCount - 1) + people[myOrder].meldedKongCount
            + people[myOrder].doraCount + people[myOrder].kingsTileDraw + people[myOrder].finalTileWin;
        totalPt.text = "合計：" + totalPoint + "翻";
    }

    //プレイ開始時, 初期画面にリセット.
    public void NotActiveSetUp()
    {
        kongImage.SetActive(false);
        meldedKongImage.SetActive(false);
        doraImage.SetActive(false);
        kingsTileImage.SetActive(false);
        finalTileImage.SetActive(false);
        totalImage.SetActive(false);
        yakumanImage.SetActive(false);
        ActivatePanel("");
        Result_Panel.SetActive(false);
        Dora_Panel.SetActive(false);
        ronEffect0.SetActive(false);
        ronEffect1.SetActive(false);
        ronEffect2.SetActive(false);
        ronEffect3.SetActive(false);
        drawEffect0.SetActive(false);
        drawEffect1.SetActive(false);
        drawEffect2.SetActive(false);
        drawEffect3.SetActive(false);
        concealedKongEffect0.SetActive(false);
        concealedKongEffect1.SetActive(false);
        concealedKongEffect2.SetActive(false);
        concealedKongEffect3.SetActive(false);
        meldedKongEffect0.SetActive(false);
        meldedKongEffect1.SetActive(false);
        meldedKongEffect2.SetActive(false);
        meldedKongEffect3.SetActive(false);

        /*
         * 4つの内下3つは表示するようになっているが, 退室ボタンクリック時に現れる用の準備.
         * そのため, trueでも最初は表示されない.
        */
        leaveCheck.SetActive(false);
        ckeckText.SetActive(true);  
        yesButton.SetActive(true);
        noButton.SetActive(true);

        reportTextObject.SetActive(false);
        ronButton.SetActive(false);
        drawButton.SetActive(false);
        meldedKongButton.SetActive(false);
        concealedKongButton.SetActive(false);
        skipButton.SetActive(false);
        selectConcealedKongTile.SetActive(false);
        furitennImage.SetActive(false);
        cantDiscardText.SetActive(false);

        notActive = true;
        isClick = false;
    }

    //槓ドラが増えたときにドラ表示UIを(枝豆から新しい槓ドラに)更新.
    public void changeDoraIndicator(int doraIndicatorNum, int initialDoraIndicatorNum)
    {
        Image image = tileController.doraIndicators[doraIndicatorNum].GetComponent<Image>();
        image.sprite = tileController.texturesOfDoraIndicator[initialDoraIndicatorNum];
    }

    //リザルト画面移行(表示)時に勝者の手牌が表示されるための仕込み.
    //勝者の手牌情報を取得し, リザルト画面の画像にコピー.
    public void changeHandOfWinner(int winnerOrder, int lastDiscardedTileNum)
    {
        GameObject container = null;
        GameObject container_Kong = null;
        switch (winnerOrder)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                container_Kong = GameObject.FindWithTag("Kong0");
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                container_Kong = GameObject.FindWithTag("Kong1");
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                container_Kong = GameObject.FindWithTag("Kong2");
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                container_Kong = GameObject.FindWithTag("Kong3");
                break;
            default:
                break;
        }
        HandContainer winnerHandContainer = container.GetComponent<HandContainer>();
        HandContainer winnerHandContainer_Kong = container_Kong.GetComponent<HandContainer>();

        //リザルト画面表示時に勝者の手牌が表示されるための仕込み.

        //対戦相手に公開していない手牌のコピー.
        if (yakuType == YAKU_TYPE.FLOWING_YAKUMANN || people[winnerOrder].ron == true)
        {
            for (int handIndex = 0; handIndex < handOfWinner.Length; handIndex++)
            {
                if (handIndex < winnerHandContainer.Count)
                {
                    int textureNum = winnerHandContainer[handIndex].tileNum;
                    Image image = handOfWinner[handIndex].GetComponent<Image>();
                    image.sprite = tileController.texturesOfDoraIndicator[textureNum];
                    handOfWinner[handIndex].SetActive(true);
                }
                else
                {
                    handOfWinner[handIndex].SetActive(false);
                }
            }
            //ロンした牌を手牌から1枚分だけ離してコピー.
            if (people[winnerOrder].ron == true)
            {
                Image image = handOfWinner[winnerHandContainer.Count + 1].GetComponent<Image>();
                image.sprite = tileController.texturesOfDoraIndicator[lastDiscardedTileNum];
                handOfWinner[winnerHandContainer.Count + 1].SetActive(true);
            }
        }
        else
        {
            for (int handIndex = 0; handIndex < handOfWinner.Length; handIndex++)
            {
                //手牌をコピーする.
                //ツモした牌を1枚分だけ離してコピーし, それ以外の不要な箇所を非表示にする.
                if (handIndex < winnerHandContainer.Count - 1)
                {
                    int textureNum = winnerHandContainer[handIndex].tileNum;
                    Image image = handOfWinner[handIndex].GetComponent<Image>();
                    image.sprite = tileController.texturesOfDoraIndicator[textureNum];
                    handOfWinner[handIndex].SetActive(true);
                }
                else if (handIndex == winnerHandContainer.Count - 1)
                {
                    int textureNum = winnerHandContainer[handIndex].tileNum;
                    Image image = handOfWinner[handIndex + 1].GetComponent<Image>();
                    image.sprite = tileController.texturesOfDoraIndicator[textureNum];
                    handOfWinner[handIndex + 1].SetActive(true);
                }
                else if (handIndex == winnerHandContainer.Count)
                {
                    handOfWinner[handIndex - 1].SetActive(false);
                }
                else
                {
                    handOfWinner[handIndex].SetActive(false);
                }
            }
        }

        //槓して全体に公開している牌のコピー.
        for (int kongIndex = 0; kongIndex < kongOfWinner.Length; kongIndex++)
        {
            if (kongIndex < winnerHandContainer_Kong.Count)
            {
                int textureNum = winnerHandContainer_Kong[kongIndex].tileNum;
                Image image = kongOfWinner[kongIndex].GetComponent<Image>();
                image.sprite = tileController.texturesOfDoraIndicator[textureNum];
                kongOfWinner[kongIndex].SetActive(true);
            }
            else
            {
                kongOfWinner[kongIndex].SetActive(false);
            }
        }
    }

    //リザルト画面への移行とリザルト処理.
    public IEnumerator StartResult(int winnerOrder)
    {
        winnerName.text = people[winnerOrder].Name;
        yield return new WaitForSeconds(1f);
        changeHandOfWinner(winnerOrder, lastDiscardedTileNum);
        Result_Panel.SetActive(true);
        yield return new WaitForSeconds(1f);
        //役の表示と役に対応したボイスを再生.
        switch (yakuType)
        {
            case YAKU_TYPE.BLESSING_OF_HEAVEN:
                ActivatePanel(BlessingOfHeaven_Panel.name);
                soundManager.soundType = SoundManager.SOUND_TYPE.WINNER;
                soundManager.PlaySoundEffect(0);
                yield return new WaitForSeconds(2f);
                yakumanImage.SetActive(true);
                soundManager.PlaySoundEffect(10);
                yield return new WaitForSeconds(2f);
                soundManager.soundType = SoundManager.SOUND_TYPE.END_VOICE;
                soundManager.PlaySoundEffect(0);
                yield return new WaitForSeconds(4.5f);
                break;
            case YAKU_TYPE.BLESSING_OF_EARTH:
                ActivatePanel(BlessingOfEarth_Panel.name);
                soundManager.soundType = SoundManager.SOUND_TYPE.WINNER;
                soundManager.PlaySoundEffect(1);
                yield return new WaitForSeconds(2f);
                yakumanImage.SetActive(true);
                soundManager.PlaySoundEffect(10);
                yield return new WaitForSeconds(2f);
                soundManager.soundType = SoundManager.SOUND_TYPE.END_VOICE;
                soundManager.PlaySoundEffect(1);
                yield return new WaitForSeconds(4.5f);
                break;
            case YAKU_TYPE.FOUR_QUADS:
                ActivatePanel(FourQuads_Panel.name);
                soundManager.soundType = SoundManager.SOUND_TYPE.WINNER;
                soundManager.PlaySoundEffect(2);
                yield return new WaitForSeconds(2f);
                yakumanImage.SetActive(true);
                soundManager.PlaySoundEffect(10);
                yield return new WaitForSeconds(2f);
                soundManager.soundType = SoundManager.SOUND_TYPE.END_VOICE;
                soundManager.PlaySoundEffect(2);
                yield return new WaitForSeconds(4.5f);
                break;
            case YAKU_TYPE.COUNTING_GRAND_SLUM:
                yakuOfKong.text = "槓　" + (people[winnerOrder].kongCount - 1) + "翻";
                yakuOfMeldedKong.text = "大明槓　" + people[winnerOrder].meldedKongCount + "翻";
                yakuOfDora.text = "ドラ　" + people[winnerOrder].doraCount + "翻";
                yakuOfKingsTile.text = "嶺上開花　" + people[winnerOrder].kingsTileDraw + "翻";
                yakuOfFinalTile.text = "海底撈月　" + people[winnerOrder].finalTileWin + "翻";
                int totalPoint = (people[winnerOrder].kongCount - 1)
                    + people[winnerOrder].meldedKongCount
                    + people[winnerOrder].doraCount
                    + people[winnerOrder].kingsTileDraw + people[winnerOrder].finalTileWin;
                yakuOfTotal.text = "計　" + totalPoint + "翻";
                ActivatePanel(CountingGrandSlum_Panel.name);
                break;
            case YAKU_TYPE.FLOWING_YAKUMANN:
                ActivatePanel(FlowingYakumann_Panel.name);
                soundManager.soundType = SoundManager.SOUND_TYPE.WINNER;
                soundManager.PlaySoundEffect(3);
                yield return new WaitForSeconds(2f);
                yakumanImage.SetActive(true);
                soundManager.PlaySoundEffect(10);
                yield return new WaitForSeconds(2f);
                soundManager.soundType = SoundManager.SOUND_TYPE.END_VOICE;
                soundManager.PlaySoundEffect(3);
                yield return new WaitForSeconds(4.5f);
                break;
            default:
                break;
        }
        if (yakuType == YAKU_TYPE.COUNTING_GRAND_SLUM)
        {
            soundManager.soundType = SoundManager.SOUND_TYPE.WINNER;
            kongImage.SetActive(true);
            soundManager.PlaySoundEffect(4);
            yield return new WaitForSeconds(1f);
            meldedKongImage.SetActive(true);
            soundManager.PlaySoundEffect(5);
            yield return new WaitForSeconds(1.5f);
            doraImage.SetActive(true);
            soundManager.PlaySoundEffect(6);
            yield return new WaitForSeconds(1f);
            kingsTileImage.SetActive(true);
            soundManager.PlaySoundEffect(7);
            yield return new WaitForSeconds(1.5f);
            finalTileImage.SetActive(true);
            soundManager.PlaySoundEffect(8);
            yield return new WaitForSeconds(1.5f);
            totalImage.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            yakumanImage.SetActive(true);
            soundManager.PlaySoundEffect(9);
            yield return new WaitForSeconds(3.5f);
            soundManager.soundType = SoundManager.SOUND_TYPE.END_VOICE;
            soundManager.PlaySoundEffect(4);
            yield return new WaitForSeconds(4.5f);
        }
    }

    //役に対応したパネルの表示.
    private void ActivatePanel(string panelToBeActivated)
    {
        BlessingOfHeaven_Panel.SetActive(panelToBeActivated.Equals(BlessingOfHeaven_Panel.name));
        BlessingOfEarth_Panel.SetActive(panelToBeActivated.Equals(BlessingOfEarth_Panel.name));
        FourQuads_Panel.SetActive(panelToBeActivated.Equals(FourQuads_Panel.name));
        CountingGrandSlum_Panel.SetActive(panelToBeActivated.Equals(CountingGrandSlum_Panel.name));
        FlowingYakumann_Panel.SetActive(panelToBeActivated.Equals(FlowingYakumann_Panel.name));
        yakumanImage.SetActive(panelToBeActivated.Equals(yakumanImage.name));
    }

    public void FetchButton()
    {
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(0);
        switch (buttonType)
        {
            case BUTTON_TYPE.DRAW:
                drawButton.SetActive(true);
                skipButton.SetActive(true);
                break;
            case BUTTON_TYPE.RON:
                ronButton.SetActive(true);
                skipButton.SetActive(true);
                break;
            case BUTTON_TYPE.MELDED_KONG:
                meldedKongButton.SetActive(true);
                skipButton.SetActive(true);
                break;
            case BUTTON_TYPE.CONCEALED_KONG:
                concealedKongButton.SetActive(true);
                skipButton.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void OnLeaveButtonClicked()
    {
        if (!isClick)
        {
            soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
            soundManager.PlaySoundEffect(3);
            leaveCheck.SetActive(true);
        }
    }

    //退室確認画面でYesボタンを押すと, ゲームの終了を対戦相手に知らせてゲームを終了する.
    public void OnYesButtonClicked()
    {
        isClick = true;
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);
        StartCoroutine(ReportAndFinishGame());
    }

    public IEnumerator ReportAndFinishGame()
    {
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(ReportEveryOne), RpcTarget.All, myOrder);
        yield return new WaitForSeconds(3f);
        photonView.RPC(nameof(EndGame), RpcTarget.All);
    }

    //退室確認画面でNoボタンを押すと, 確認画面を非表示にする.
    public void OnNoButtonClicked()
    {
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(2);
        leaveCheck.SetActive(false);
    }

    public void OnDrawButtonClicked()
    {
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);
        skipButton.SetActive(false);
        drawButton.SetActive(false);
        StartCoroutine(ReadyToWin());
        StartCoroutine(OnDrawEffect(myOrder));
    }

    public void OnRonButtonClicked()
    {
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);
        skipButton.SetActive(false);
        ronButton.SetActive(false);
        StartCoroutine(ReadyToWin());
        StartCoroutine(OnRonEffect(myOrder));
    }

    //処理中に次の処理を開始して、バグってしまうことがある.
    //コルーチンで処理完了を待ってから次の処理を行う.

    /*
     * 和了った時に自分の手牌を麻雀台にまっすぐ立たせる.
     * なぜなら, 見やすいように傾けられているから.
     * 牌のtransformを変えると手牌の順序が追加された順になるのでソートし直す.
    */
    IEnumerator ReadyToWin()
    {
        GameObject container = null;
        HandContainer handContainer = null;
        switch (myOrder)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                handContainer = container.GetComponent<HandContainer>();
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                handContainer = container.GetComponent<HandContainer>();
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                handContainer = container.GetComponent<HandContainer>();
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                handContainer = container.GetComponent<HandContainer>();
                break;
            default:
                break;
        }

        //自分の手牌を麻雀台にまっすぐ立たせ, ソートし直す.
        bool firstReady = false;
        for (int i = 0; i < handContainer.Count; i++)
        {
            handContainer[i].order = -1;
            handContainer[i].transform.position = handContainer[i].initialHandPosition;
            if (i == handContainer.Count - 1)
            {
                firstReady = true;
            }
        }
        yield return new WaitUntil(() => firstReady);
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        TileController tileController = gameController.GetComponent<TileController>();
        tileController.easySortedHand = false;
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(tileController.EasySortHand), RpcTarget.All, myOrder);
        yield return new WaitUntil(() => tileController.easySortedHand);
    }

    public void OnConcealedKongButtonClicked()
    {
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);
        skipButton.SetActive(false);
        concealedKongButton.SetActive(false);
        //槓した後新しい牌を引くまで操作不可能にするために牌のmyTurnEndをtrueにしている.
        foreach(HandContainerChild handContainerChild in myHandContainer)
        {
            handContainerChild.myTurnEnd = true;
        }
        people[myOrder].didConcealedKong = true;
        people[myOrder].judged = true;
    }

    public void ChangeSelectConcealedImages(int kongNum0, int kongNum1)
    {
        Image image0 = tileImage0.GetComponent<Image>();
        Image image1 = tileImage1.GetComponent<Image>();
        image0.sprite = tileController.texturesOfDoraIndicator[kongNum0];
        image1.sprite = tileController.texturesOfDoraIndicator[kongNum1];
    }

    public void OnConcealedKong0Clicked()
    {
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);
        selectConcealedKongTile.SetActive(false);
        gameManager.concealedKongTileNum = checker.canConcealedKongTileNum[0];
        gameManager.selectedConcealedKong = true;
    }

    public void OnConcealedKong1Clicked()
    {
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);
        selectConcealedKongTile.SetActive(false);
        gameManager.concealedKongTileNum = checker.canConcealedKongTileNum[1];
        gameManager.selectedConcealedKong = true;
    }

    public void OnMeldedKongButtonClicked()
    {
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);
        skipButton.SetActive(false);
        meldedKongButton.SetActive(false);
        StartCoroutine(OnMeldedKongEffect(myOrder));
    }

    public void OnSkipButtonClicked()
    {
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(2);
        drawButton.SetActive(false);
        ronButton.SetActive(false);
        meldedKongButton.SetActive(false);
        concealedKongButton.SetActive(false);
        skipButton.SetActive(false);
        //スキップ情報をネットワーク越しに同期.
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(SendJudgeOfSkip), RpcTarget.All, myOrder);
    }

    public IEnumerator OnDrawEffect(int orderOfDrawer)
    {
        yield return new WaitForSeconds(0.05f);
        soundManager.soundType = SoundManager.SOUND_TYPE.CHARACTER;
        soundManager.PlaySoundEffect(0);
        //エフェクトのOnOff切り替えを同期.
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(SwitchDrawEffect), RpcTarget.All, orderOfDrawer, true);
        yield return new WaitForSeconds(0.5f);
        photonView.RPC(nameof(SwitchDrawEffect), RpcTarget.All, orderOfDrawer, false);
        yield return new WaitForSeconds(0.5f);
        people[orderOfDrawer].draw = true;
        people[orderOfDrawer].judged = true;
    }

    public IEnumerator OnRonEffect(int ronOrder)
    {
        yield return new WaitForSeconds(0.05f);
        soundManager.soundType = SoundManager.SOUND_TYPE.CHARACTER;
        soundManager.PlaySoundEffect(1);
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(SwitchRonEffect), RpcTarget.All, ronOrder, true);
        yield return new WaitForSeconds(0.5f);
        photonView.RPC(nameof(SwitchRonEffect), RpcTarget.All, ronOrder, false);
        yield return new WaitForSeconds(0.5f);
        photonView.RPC(nameof(SendJudgeOfRon), RpcTarget.All, ronOrder);
    }

    public IEnumerator OnConcealedKongEffect(int conealedKongOrder)
    {
        yield return new WaitForSeconds(0.05f);
        soundManager.soundType = SoundManager.SOUND_TYPE.CHARACTER;
        soundManager.PlaySoundEffect(2);
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(SwitchConcealedKongEffect), RpcTarget.All, conealedKongOrder, true);
        yield return new WaitForSeconds(0.5f);
        photonView.RPC(nameof(SwitchConcealedKongEffect), RpcTarget.All, conealedKongOrder, false);
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator OnMeldedKongEffect(int  meldedKongOrder)
    {
        yield return new WaitForSeconds(0.05f);
        soundManager.soundType = SoundManager.SOUND_TYPE.CHARACTER;
        soundManager.PlaySoundEffect(3);
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(SwitchMeldedKongEffect), RpcTarget.All, meldedKongOrder, true);
        yield return new WaitForSeconds(0.5f);
        photonView.RPC(nameof(SwitchMeldedKongEffect), RpcTarget.All, meldedKongOrder, false);
        yield return new WaitForSeconds(0.5f);
        photonView.RPC(nameof(SendJudgeOfMeldedKong), RpcTarget.All, meldedKongOrder);
    }

    //[PunRPC]は関数を同期するためのもの.
    //オブジェクトをすべて同期しているわけではない
    //そのため, 関数を同期するには毎回そのスクリプトを取得し, 実行する必要がある.

    //ゲームの終了選択を知らせる.
    [PunRPC]
    public void ReportEveryOne(int order)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        UIController uiController = gameController.GetComponent<UIController>();
        uiController.ckeckText.SetActive(false);
        uiController.yesButton.SetActive(false);
        uiController.noButton.SetActive(false);
        uiController.reportTextObject.SetActive(true);
        reportText.text = people[order].Name + "さんによって" + "\n"
            + "ゲームの終了が選択されました。" + "\n"
            + "3秒後にゲームを終了します。";
        uiController.leaveCheck.SetActive(true);
    }

    //誰かがゲームから離脱し、ゲームが終了することを知らせる.
    [PunRPC]
    public void ReportDestroyGame()
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        UIController uiController = gameController.GetComponent<UIController>();
        uiController.ckeckText.SetActive(false);
        uiController.yesButton.SetActive(false);
        uiController.noButton.SetActive(false);
        uiController.reportTextObject.SetActive(true);
        reportText.text = "他のプレイヤーが" + "\n"
            + "ゲームからログアウトました。" + "\n"
            + "3秒後にゲームを終了します。";
        uiController.leaveCheck.SetActive(true);
    }

    [PunRPC]
    public void EndGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }

    [PunRPC]
    public void SwitchDrawEffect(int orderOfDrawer, bool boolSwitch)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        UIController uiController = gameController.GetComponent<UIController>();
        //自分から見てどの人がツモしたのかを計算する式.
        int drawPosition
            = (orderOfDrawer - uiController.myOrder + uiController.people.Count)
                % uiController.people.Count;
        uiController.drawEffects[drawPosition].SetActive(boolSwitch);
    }

    [PunRPC]
    public void SwitchRonEffect(int ronOrder, bool boolSwitch)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        UIController uiController = gameController.GetComponent<UIController>();
        int ronPosition
            = (ronOrder - uiController.myOrder + uiController.people.Count)
                % uiController.people.Count;
        uiController.ronEffects[ronPosition].SetActive(boolSwitch);
    }

    [PunRPC]
    public void SwitchConcealedKongEffect(int conealedKongOrder, bool boolSwitch)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        UIController uiController = gameController.GetComponent<UIController>();
        int conealedKongPosition
            = (conealedKongOrder - uiController.myOrder + uiController.people.Count)
                % uiController.people.Count;
        uiController.concealedKongEffects[conealedKongPosition].SetActive(boolSwitch);
    }

    [PunRPC]
    public void SwitchMeldedKongEffect(int meldedKongOrder, bool boolSwitch)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        UIController uiController = gameController.GetComponent<UIController>();
        int meldedKongPosition
            = (meldedKongOrder - uiController.myOrder + uiController.people.Count)
                % uiController.people.Count;
        uiController.meldedKongEffects[meldedKongPosition].SetActive(boolSwitch);
    }


    [PunRPC]
    public void SendJudgeOfRon(int myOrder)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.people[myOrder].ron = true;
        gameManager.people[myOrder].judged = true;
    }

    [PunRPC]
    public void SendJudgeOfMeldedKong(int myOrder)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.people[myOrder].didMeldedKong = true;
        gameManager.people[myOrder].judged = true;
    }

    [PunRPC]
    public void SendJudgeOfSkip(int myOrder)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.people[myOrder].judged = true;
    }

}