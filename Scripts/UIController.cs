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
    public HandContainer myHandContainer;   //��v�̐e�I�u�W�F�N�g.
    public GameManager gameManager;
    public Checker checker;     //�𔻒�X�N���v�g.
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
        BLESSING_OF_HEAVEN = 0,     //�V�a.
        BLESSING_OF_EARTH,          //�n�a.
        FOUR_QUADS,                 //�l�Ȏq.
        COUNTING_GRAND_SLUM,        //������.
        FLOWING_YAKUMANN,           //�����.
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
    public GameObject meldedKongButton;     //�喾�ȃ{�^��.
    public GameObject concealedKongButton;  //�Þȃ{�^��.
    public GameObject skipButton;
    public GameObject selectConcealedKongTile;
    public GameObject tileImage0;   //�ÞȂł���v��2�̎���1��.
    public GameObject tileImage1;   //�ÞȂł���v��2�̎���2��.

    public GameObject leaveCheck;
    public GameObject ckeckText;
    public GameObject yesButton;
    public GameObject noButton;
    public GameObject reportTextObject;
    public TextMeshProUGUI reportText;

    [Header("Effect Panel Info")]
    public GameObject ronEffect0;   //�����̃����G�t�F�N�g.
    public GameObject ronEffect1;   //�E���̐l�̃����G�t�F�N�g.
    public GameObject ronEffect2;   //�Ζʂ̐l�̃����G�t�F�N�g.
    public GameObject ronEffect3;   //�����̐l�̃����G�t�F�N�g.
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
         * �h���\��UI�̐ݒ�.
         * �h���\������Ă��Ȃ�����(����14��)���}���ŕ\������.
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
        wallTilesNum.text = "�v�R" + "\n" + "�c�F" + wallTilesCount + "��";
        kongPt.text = "�ȁF" + (people[myOrder].kongCount - 1) + "�|";
        meldedKongPt.text = "�喾�ȁF" + people[myOrder].meldedKongCount + "�|";
        doraPt.text = "�h���F" + people[myOrder].doraCount + "�|";
        kingsTilePt.text = "���J�ԁF" + people[myOrder].kingsTileDraw + "�|";
        finalTilePt.text = "�C�ꝝ���F" + people[myOrder].finalTileWin + "�|";
        int totalPoint = (people[myOrder].kongCount - 1) + people[myOrder].meldedKongCount
            + people[myOrder].doraCount + people[myOrder].kingsTileDraw + people[myOrder].finalTileWin;
        totalPt.text = "���v�F" + totalPoint + "�|";
    }

    //�v���C�J�n��, ������ʂɃ��Z�b�g.
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
         * 4�̓���3�͕\������悤�ɂȂ��Ă��邪, �ގ��{�^���N���b�N���Ɍ����p�̏���.
         * ���̂���, true�ł��ŏ��͕\������Ȃ�.
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

    //�ȃh�����������Ƃ��Ƀh���\��UI��(�}������V�����ȃh����)�X�V.
    public void changeDoraIndicator(int doraIndicatorNum, int initialDoraIndicatorNum)
    {
        Image image = tileController.doraIndicators[doraIndicatorNum].GetComponent<Image>();
        image.sprite = tileController.texturesOfDoraIndicator[initialDoraIndicatorNum];
    }

    //���U���g��ʈڍs(�\��)���ɏ��҂̎�v���\������邽�߂̎d����.
    //���҂̎�v�����擾��, ���U���g��ʂ̉摜�ɃR�s�[.
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

        //���U���g��ʕ\�����ɏ��҂̎�v���\������邽�߂̎d����.

        //�ΐ푊��Ɍ��J���Ă��Ȃ���v�̃R�s�[.
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
            //���������v����v����1�������������ăR�s�[.
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
                //��v���R�s�[����.
                //�c�������v��1�������������ăR�s�[��, ����ȊO�̕s�v�ȉӏ����\���ɂ���.
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

        //�Ȃ��đS�̂Ɍ��J���Ă���v�̃R�s�[.
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

    //���U���g��ʂւ̈ڍs�ƃ��U���g����.
    public IEnumerator StartResult(int winnerOrder)
    {
        winnerName.text = people[winnerOrder].Name;
        yield return new WaitForSeconds(1f);
        changeHandOfWinner(winnerOrder, lastDiscardedTileNum);
        Result_Panel.SetActive(true);
        yield return new WaitForSeconds(1f);
        //���̕\���Ɩ��ɑΉ������{�C�X���Đ�.
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
                yakuOfKong.text = "�ȁ@" + (people[winnerOrder].kongCount - 1) + "�|";
                yakuOfMeldedKong.text = "�喾�ȁ@" + people[winnerOrder].meldedKongCount + "�|";
                yakuOfDora.text = "�h���@" + people[winnerOrder].doraCount + "�|";
                yakuOfKingsTile.text = "���J�ԁ@" + people[winnerOrder].kingsTileDraw + "�|";
                yakuOfFinalTile.text = "�C�ꝝ���@" + people[winnerOrder].finalTileWin + "�|";
                int totalPoint = (people[winnerOrder].kongCount - 1)
                    + people[winnerOrder].meldedKongCount
                    + people[winnerOrder].doraCount
                    + people[winnerOrder].kingsTileDraw + people[winnerOrder].finalTileWin;
                yakuOfTotal.text = "�v�@" + totalPoint + "�|";
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

    //���ɑΉ������p�l���̕\��.
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

    //�ގ��m�F��ʂ�Yes�{�^����������, �Q�[���̏I����ΐ푊��ɒm�点�ăQ�[�����I������.
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

    //�ގ��m�F��ʂ�No�{�^����������, �m�F��ʂ��\���ɂ���.
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

    //�������Ɏ��̏������J�n���āA�o�O���Ă��܂����Ƃ�����.
    //�R���[�`���ŏ���������҂��Ă��玟�̏������s��.

    /*
     * �a���������Ɏ����̎�v�𖃐���ɂ܂�������������.
     * �Ȃ��Ȃ�, ���₷���悤�ɌX�����Ă��邩��.
     * �v��transform��ς���Ǝ�v�̏������ǉ����ꂽ���ɂȂ�̂Ń\�[�g������.
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

        //�����̎�v�𖃐���ɂ܂�����������, �\�[�g������.
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
        //�Ȃ�����V�����v�������܂ő���s�\�ɂ��邽�߂ɔv��myTurnEnd��true�ɂ��Ă���.
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
        //�X�L�b�v�����l�b�g���[�N�z���ɓ���.
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(SendJudgeOfSkip), RpcTarget.All, myOrder);
    }

    public IEnumerator OnDrawEffect(int orderOfDrawer)
    {
        yield return new WaitForSeconds(0.05f);
        soundManager.soundType = SoundManager.SOUND_TYPE.CHARACTER;
        soundManager.PlaySoundEffect(0);
        //�G�t�F�N�g��OnOff�؂�ւ��𓯊�.
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

    //[PunRPC]�͊֐��𓯊����邽�߂̂���.
    //�I�u�W�F�N�g�����ׂē������Ă���킯�ł͂Ȃ�
    //���̂���, �֐��𓯊�����ɂ͖��񂻂̃X�N���v�g���擾��, ���s����K�v������.

    //�Q�[���̏I���I����m�点��.
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
        reportText.text = people[order].Name + "����ɂ����" + "\n"
            + "�Q�[���̏I�����I������܂����B" + "\n"
            + "3�b��ɃQ�[�����I�����܂��B";
        uiController.leaveCheck.SetActive(true);
    }

    //�N�����Q�[�����痣�E���A�Q�[�����I�����邱�Ƃ�m�点��.
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
        reportText.text = "���̃v���C���[��" + "\n"
            + "�Q�[�����烍�O�A�E�g�܂����B" + "\n"
            + "3�b��ɃQ�[�����I�����܂��B";
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
        //�������猩�Ăǂ̐l���c�������̂����v�Z���鎮.
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