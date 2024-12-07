using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using ExitGames.Client.Photon;


/*
 * ����!!!
 * �������d�v�ȃ^�[�����Q�[���Ȃ̂ŃR���[�`���œ�������������܂őҋ@����𑽁̂X�g�p���Ă���.
 * �Ⴆ��, ��v�̓����̏����ƃ\�[�g�̓����̏������قړ����Ɏn�܂��,
 * ��v�̓����ƃ\�[�g�̓��������񏈗�����, ����������񂪊Ԉ�������̂ɂȂ�.
 * ��v��1�ڂ̔v�𓯊�����ԂɃ\�[�g����, 2�ڂ̔v��1�ڂ̔v�Ɠ����v�Ȃ̂ɓ��������Ȃǂ��N����.
 * ���̂��ߓ���������̂�����ꍇ�͊��S�ɏ������I���̂��܂��Ă���, ���̏������s���K�v������.
*/


//IPunTurnManagerCallbacks�ŊȒP�Ƀ^�[�����Q�[���̎������ł��邪,
//CPU�̏����������ɂł�����������ׂĂ�(�ł��邩�ǂ���)�킩��Ȃ���������.
//�g�p������, �����Ŏ������邱�Ƃ�.
public class GameManager : MonoBehaviourPunCallbacks//, IPunTurnManagerCallbacks
{
    #region Setting Variable
    public const int numPlayers = 4;
    public bool startedGame = false;
    public bool isGameSet = false;
    public int nowTurn = -1;
    private List<int> initialList = new List<int> { 0, 1, 2, 3 };
    public List<int> orders;
    public List<int> types;
    private const string NamesKey = "n"; //Key�̕������������Ɠ����̏������d���Ȃ�̂�1�����������Ă���.
    private const string IdsKey = "i";
    private const string OrdersKey = "o";
    private const string TypesKey = "t";    
    private const string MainHandsKey = "m";    
    private const string HandsKey = "h";
    private const string WallTilesKey = "w";
    private const string DeadWallKey = "d";
    private ExitGames.Client.Photon.Hashtable namesHashtable;
    private ExitGames.Client.Photon.Hashtable idsHashtable;
    private ExitGames.Client.Photon.Hashtable ordersHashtable;
    private ExitGames.Client.Photon.Hashtable typesHashtable;   //�v�̎��. 1�F�ݎq, 2�F���q, 3�F���q, 4�F���v�̏��.
    private ExitGames.Client.Photon.Hashtable mainHandsHashtable;   //�Ȃł��铯��3�̔v�̐���4�g�̏��.
    private ExitGames.Client.Photon.Hashtable handsHashtable;
    private ExitGames.Client.Photon.Hashtable allHashtables;
    private int hashtableUpdateCount;
    public string myName;
    public int myId;
    public int myOrder;
    public int myType;
    public int[] myMainHandArray;
    public int[] myHandArray;
    public int[] wallTilesArray; 
    public int[] deadWallArray;
    public List<int> myHand;
    public List<int> wallTiles;     //�v�R(���R�D).
    public List<int> deadWall;      //���v(�h���\���v).
    public List<int> doraList;      //���J�����h���\���v.
    public WallTilesModel wallTilesModel;
    public CameraController cameraController;
    public PeopleModel peopleModel;
    public List<Person> people;
    public TileController tileController;
    public HandContainer doraContainer;
    public SoundManager soundManager;
    #endregion

    //��v��J�����Ȃǂ̏����ݒ���s��.
    void Awake()
    {
        hashtableUpdateCount = 0;
        myName = PhotonNetwork.LocalPlayer.NickName;
        myId = PhotonNetwork.LocalPlayer.ActorNumber;
        Player[] players = PhotonNetwork.PlayerList;
        namesHashtable = new ExitGames.Client.Photon.Hashtable();
        idsHashtable = new ExitGames.Client.Photon.Hashtable();
        ordersHashtable = new ExitGames.Client.Photon.Hashtable();
        typesHashtable = new ExitGames.Client.Photon.Hashtable();
        mainHandsHashtable = new ExitGames.Client.Photon.Hashtable();
        handsHashtable = new ExitGames.Client.Photon.Hashtable();
        allHashtables = new ExitGames.Client.Photon.Hashtable();
        cameraController = GetComponent<CameraController>();
        cameraController.gameStarting = true; 
        startedGame = false;
        isGameSet = false;
        nowTurn = -1;
        winnerOrder = -1;
        doraCounter = 0;
        drawCount = 0;
        lastDiscardedTileNum = -1;
        lastDiscardedTile = null;
        tileController = GetComponent<TileController>();
        soundManager = GetComponent<SoundManager>();
        checker = GetComponent<Checker>();
        checker.CheckerSetting();
        uiController = GetComponent<UIController>();

        //�l�b�g���[�N��ŕϐ���֐��𓯊����邽�߂̃X�N���v�g.
        PhotonView photonView = GetComponent<PhotonView>(); 
        //��v��v�R, ���v�Ȃǂ̍쐬���}�X�^�[�N���C�A���g���s��.
        //��v�������ꂼ��̃v���C���[���s���Ƃ��̓x�ɓ�������K�v�����邽�߃z�X�g�����ׂčs��.
        //�Ⴆ�Δv�R�̏�񂪍X�V�����O�ɔv�������Ă��܂�, �����v�������o�O����������\��������.
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            wallTilesModel = GetComponent<WallTilesModel>();
            wallTilesModel.MakeWallTiles(numPlayers);
            wallTiles = wallTilesModel.wallTiles;
            deadWall = wallTilesModel.deadWall;
            DecidePeapleInfo(players);
            PhotonNetwork.CurrentRoom.SetCustomProperties(allHashtables);   
            //Hashtable(�ϐ��⃊�X�g)���𑗐M���Ă���.
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForRoomPropertiesUpdate());
    }

    // Update is called once per frame
    void Update()
    {
        if (!startedGame && nowTurn != -1)
        {
            startedGame = true;
            StartCoroutine(Battle());
        }
    }

    #region InitialSettUp

    void DecidePeapleInfo(Player[] players)
    {
        orders = Shuffle(initialList);  //�v���C��.
        types = Shuffle(initialList);   //��Ȏ�v�̎��.
        createHashtables(players);
        //ActorNumber�̓l�b�g���[�N���ID. CPU�������Ȃ��̂� name �� id.
        //��v�Ȃǂ̏������삷��.
    }

    public List<int> Shuffle(List<int> list)
    {
        List<int> copyList = new List<int> (list);
        for(int i = 0; i < list.Count; i++)
        {
            int k = Random.Range(0, list.Count);
            int temp = copyList[k];
            copyList[k] = copyList[i];
            copyList[i] = temp;
        }
        return copyList;
    }

    void createHashtables(Player[] players)
    {
        //ActorNumber�̓l�b�g���[�N���ID. CPU�������Ȃ��̂� name �� id.
        //��v�Ȃǂ̏������삷��.
        for (int i = 0; i < players.Length; i++)
        {
            namesHashtable.Add(orders[i].ToString(), players[i].NickName);
            idsHashtable.Add(orders[i].ToString(), players[i].ActorNumber); 
            ordersHashtable.Add(players[i].ActorNumber.ToString(), orders[i]);
            typesHashtable.Add(players[i].ActorNumber.ToString(), types[i]);
            int[] mainHandArray = wallTilesModel.mainHands[i].ToArray();
            mainHandsHashtable.Add(i.ToString(), mainHandArray);
            int[] handArray = wallTilesModel.hands[i].ToArray();
            handsHashtable.Add(i.ToString(), handArray);
        }
        //CPU�̏��ǉ�.
        for (int i = 0; i < numPlayers - players.Length; i++)
        {
            namesHashtable.Add(orders[i + players.Length].ToString(), "CPU" + i);
            idsHashtable.Add(orders[i + players.Length].ToString(), -i);
            ordersHashtable.Add((-i).ToString(), orders[i + players.Length]);
            typesHashtable.Add((-i).ToString(), types[i + players.Length]);
            int[] mainHandArray = wallTilesModel.mainHands[i + players.Length].ToArray();
            mainHandsHashtable.Add((i + players.Length).ToString(), mainHandArray);
            int[] handArray = wallTilesModel.hands[i + players.Length].ToArray();
            handsHashtable.Add((i + players.Length).ToString(), handArray);
        }

        //�e Hashtable �� allHashtables �Ɋi�[.
        //���̎� Key�̕������������Ɠ����̏������d���Ȃ�̂�1�����������Ă���.
        allHashtables.Add(NamesKey, namesHashtable);
        allHashtables.Add(IdsKey, idsHashtable);
        allHashtables.Add(OrdersKey, ordersHashtable);
        allHashtables.Add(TypesKey, typesHashtable);
        allHashtables.Add(MainHandsKey, mainHandsHashtable);
        allHashtables.Add(HandsKey, handsHashtable);
        wallTilesArray = wallTiles.ToArray();
        allHashtables.Add(WallTilesKey, wallTilesArray);
        deadWallArray = deadWall.ToArray();
        allHashtables.Add(DeadWallKey, deadWallArray);
    }

    //Hashtable �X�V�� Hashtable ���R�s�[����.
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        allHashtables = propertiesThatChanged;
        if(hashtableUpdateCount == 0)
        {
            namesHashtable = (ExitGames.Client.Photon.Hashtable)allHashtables[NamesKey];
            idsHashtable = (ExitGames.Client.Photon.Hashtable)allHashtables[IdsKey];
            ordersHashtable = (ExitGames.Client.Photon.Hashtable)allHashtables[OrdersKey];
            typesHashtable = (ExitGames.Client.Photon.Hashtable)allHashtables[TypesKey];
            mainHandsHashtable = (ExitGames.Client.Photon.Hashtable)allHashtables[MainHandsKey];
            handsHashtable = (ExitGames.Client.Photon.Hashtable)allHashtables[HandsKey];
            wallTilesArray = (int[])allHashtables[WallTilesKey];
            deadWallArray = (int[])allHashtables[DeadWallKey];
            hashtableUpdateCount++;
        }
    }

    //Hashtable �̃R�s�[���ςނƃ��[�J���v���C���[��v�R�Ȃǂ̏���ݒ肷��.
    //�܂�, ��v�Ȃǂ̃v���C���[���̏����i�[���邽�߂Ƀv���C���[�I�u�W�F�N�g���쐬����.
    //�����, �l�b�g���[�N��Ńv���C���[�̃I�u�W�F�N�g�͑��݂��邪CPU�p�����݂��Ȃ�����쐬����.
    IEnumerator WaitForRoomPropertiesUpdate()
    {
        while (allHashtables == null || !allHashtables.ContainsKey(OrdersKey))
        {
            yield return null;
        }

        myOrder = (int) ordersHashtable[myId.ToString()];
        myType = (int) typesHashtable[myId.ToString()];
        myMainHandArray = (int[]) mainHandsHashtable[myType.ToString()];
        myHandArray = (int[]) handsHashtable[myType.ToString()];
        myHand = new List<int>(myHandArray);
        wallTiles = new List<int>(wallTilesArray);
        deadWall = new List<int>(deadWallArray);
        doraList = new List<int> { 0, 10, 20 };
        cameraController.PlayingCameraTransform(myOrder);

        peopleModel = GetComponent<PeopleModel>();
        peopleModel.MakePeople(namesHashtable, idsHashtable, 
            typesHashtable, mainHandsHashtable, handsHashtable);
        people = peopleModel.people;
        uiController.notActive = false;
        uiController.SetUpInitialUI(myOrder, people, wallTiles.Count);
        yield return  new WaitUntil(() => uiController.notActive);

        StartCoroutine(InitialSetUp());
    }

    #endregion InitialSetUp

    #region InitialSetUpMethod
    IEnumerator InitialSetUp()
    {
        //�����̃Z�b�g�A�b�v�̏�����҂��Ȃ��Ǝ�v���X���鏈�����x��čs���.
        //�X�y�b�N�̒ႢPC�ŎQ������Ƒ��l�̎�v��������o�O����������.
        //�����������邽�߂ɒZ���ԏ�����҂�.
        yield return new WaitForSeconds(0.5f);
        //�v���Ƀl�b�g���[�N���id�����蓖�ĂĂ������ߑ傫�߂̐���(��̈�)�ŏ��L�҂���ʂ���.
        tileController.viewId = 1000 * (myOrder + 1);  
        if (PhotonNetwork.IsMasterClient)
        {
            yield return StartCoroutine(DoraSetUp());
        }
        yield return StartCoroutine(HandsSetUp());
        yield return StartCoroutine(SortHands());
        if (PhotonNetwork.IsMasterClient)
        {
            /*
             * �֐��𓯊����邽�߂̃R�[�h. ����������������֐�. 
             * �������͑S���������ȊO���Ŏ������邱�Ƃ�I�����邱�Ƃ��ł���.
             * �����ȊO�̏ꍇ�ʂŎ����̏������K�v�Ȃ̂�, 
             * �Ȃ������܂��������ꂸ�o�O����������̂őS���Ŏ��s���Ă���.
            */
            photonView.RPC(nameof(StartGame), RpcTarget.All);
         }
        yield return new WaitUntil(() => nowTurn != -1);
    }

    IEnumerator DoraSetUp()
    {
        tileController.instantiatedDora = false;
        tileController.InstantiateDora(deadWall);
        yield return new WaitUntil(() => tileController.instantiatedDora);
    }

    IEnumerator HandsSetUp()
    {
        for(int i = 0;  i < numPlayers; i++)
        {
            //�}�X�^�[�N���C�A���g�͎��g��CPU�̎�v�I�u�W�F�N�g���o��������.
            if (PhotonNetwork.IsMasterClient && !(people[i].isHuman))
            {
                tileController.instantiatedHand = false;
                tileController.InstantiateHand(people[i].order, people[i].hand);
                yield return new WaitUntil(() => tileController.instantiatedHand);
            }
            else if(people[i].order == myOrder)
            {
                tileController.instantiatedHand = false;
                tileController.InstantiateHand(people[i].order, people[i].hand);
                yield return new WaitUntil(() => tileController.instantiatedHand);
            }
        }
    }

    IEnumerator SortHands()
    {
        for (int i = 0; i < numPlayers; i++)
        {
            if (PhotonNetwork.IsMasterClient && !(people[i].isHuman))
            {
                tileController.sortedHand = false;
                photonView.RPC(nameof(tileController.SortHand), RpcTarget.All, people[i].order);
                yield return new WaitUntil(() => tileController.sortedHand);
            }
            else if (people[i].order == myOrder)
            {
                tileController.sortedHand = false;
                photonView.RPC(nameof(tileController.SortHand), RpcTarget.All, people[i].order);
                yield return new WaitUntil(() => tileController.sortedHand);
            }
        }
        yield return new WaitForSeconds(0.5f);
    }

    //[PunRPC]�͊֐��𓯊����邽�߂̂���.
    //�I�u�W�F�N�g����ɓ������Ă���킯�ł͂Ȃ��̂Ń��\�b�h�̎��s��ϐ��̕ύX�����s�������ꍇ��.
    //�X�N���v�g���擾���Ă�����s����K�v������.
    [PunRPC]
    private void StartGame()
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.nowTurn = 0;
    }

    #endregion

    #region VariableForPlayingMainGame
    public bool discarded = false;  //��v���̂Ă���.
    public bool removedWallTiles = false; 
    public bool updatedDrawPoint = false;   //���J�ԂƊC�ꝝ�����X�V�ς݂�.
    public bool didPlusDrawCount = false;   //�v���������񐔂̍��v���v�Z�ς݂��ǂ���.
    public bool changedUI = false;
    public bool selectedConcealedKong = false;
    public bool changedFuritennState = false; 
    public bool changedDidMeldedKong = false; 
    public bool nextReady = false;
    public bool wasGameSet = false;
    public bool wentNextTurn = false;
    public int doraCounter = 0;
    public int concealedKongTileNum;
    public int winnerOrder = -1;
    //�V�a, �n�a�͞Ȃ��ꂸ�ɍŏ��̈������v�ŏオ�ꂽ���݂̂Ȃ̂�4�܂ŃJ�E���g����.
    public int drawCount = 0;   
    public int lastDiscardedTileNum = -1;
    public GameObject lastDiscardedTile = null;
    public Checker checker;     //�𔻒�X�N���v�g.
    public UIController uiController;
    #endregion


    IEnumerator Battle()
    {
        yield return new WaitForSeconds(0.5f);

        while (!isGameSet)
        {
            if (PhotonNetwork.IsMasterClient && !(people[nowTurn].isHuman))
            {
                yield return StartCoroutine(PlayTurn());
            }
            else if (nowTurn == myOrder)
            {
                yield return StartCoroutine(PlayTurn());
            }
            yield return null;
        }
        //���U���g����. ���̌ネ�r�[�ɖ߂�.
        yield return new WaitUntil(() => (winnerOrder != -1));
        yield return StartCoroutine(uiController.StartResult(winnerOrder));
        yield return new WaitForSeconds(1f);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }

    IEnumerator LeaveRoomAndLoadScene(string sceneName)
    {
        PhotonNetwork.LeaveRoom();
        yield return new WaitUntil(() => !PhotonNetwork.InRoom); // Photon ���[������ޏo����̂�ҋ@.
        SceneManager.LoadScene(sceneName); 
    }

    IEnumerator PlayTurn()
    {
        discarded = false;
        if (!people[nowTurn].didMeldedKong)
        {
            //�ʏ펞�͔v�R�̍ŏ��������.
            yield return StartCoroutine(DrawWallTile(0, true));
            yield return StartCoroutine(JudgeBlessing());
            if (isGameSet)
            {
                yield break;
            }
        }
        else
        {
            //�Ȃ����Ƃ��͔v�R�̍Ōォ�����.
            yield return StartCoroutine(DrawWallTile(wallTiles.Count - 1, false));
        }
        yield return StartCoroutine(JudgeDrawFourQuads());
        if (isGameSet)
        {
            yield break;
        }
        yield return StartCoroutine(JudgeCountingGrandSlum());
        if (isGameSet)
        {
            yield break;
        }

        yield return StartCoroutine(JudgeConcealedKong());
        if (isGameSet)
        {
            yield break;
        }

        //�̂Ĕv���͑҂�.
        //CPU�͎����Ŏ�v����1�I��Ŏ̂Ă�����.
        if (!(people[nowTurn].isHuman))
        {
            yield return StartCoroutine(CPUDiscardTile());
        }
        yield return new WaitUntil(() => discarded);

        yield return StartCoroutine(JudgeFuritenn());
        yield return StartCoroutine(SortingAfterDiscarded());

        //14��ڂ܂ő喾�Ȍ�Ƀh�����߂���, �S���v���C���[�̃h���𐔂���.
        if (people[nowTurn].didMeldedKong && doraCounter < tileController.doraIndicators.Length)
        {
            yield return StartCoroutine(AddingDora());
        }
        yield return StartCoroutine(DoraCount(nowTurn));

        yield return StartCoroutine(WaitChangingDidMeldedKong
            (nowTurn, false, false));

        //�喾�Ȃ�胍�����D�悳���̂Ń����ł��邩�ǂ����̏������ɍs��.
        yield return StartCoroutine(JudgeRonFourQuads());
        if (isGameSet)
        {
            yield break;
        }
        yield return StartCoroutine(JudgeMeldedKong());

        yield return StartCoroutine(AdvanceTurn());
    }

    IEnumerator DrawWallTile(int drawIndex, bool head)
    {
        tileController.instantiatedHand = false;
        StartCoroutine(tileController.Draw(nowTurn, wallTiles[drawIndex]));
        yield return new WaitUntil(() => tileController.instantiatedHand);

        removedWallTiles = false;
        photonView.RPC(nameof(RemoveWallTiles), RpcTarget.All, drawIndex);
        yield return new WaitUntil(() => removedWallTiles);

        int finalTileWinPoint;  //�C�ꝝ��(�v�R�Ō�̔v�ŏオ�������ɂ���).
        int kingsTileDrawPoint; //���J��(�Ȃ��Ĉ������Ƃ��ɏオ���ƕt����).
        bool endDraw = false;
        if(head && wallTiles.Count == 0)
        {
            updatedDrawPoint = false;
            finalTileWinPoint = 1;
            kingsTileDrawPoint = 0;
            photonView.RPC(nameof(UpdateDrawPoint), RpcTarget.All, nowTurn, 
                finalTileWinPoint, kingsTileDrawPoint);
            yield return new WaitUntil(() => updatedDrawPoint);
            endDraw = true;
        }
        else if(!head)
        {
            updatedDrawPoint = false;
            finalTileWinPoint = 0;
            kingsTileDrawPoint = 1;
            photonView.RPC(nameof(UpdateDrawPoint), RpcTarget.All, nowTurn,
                finalTileWinPoint, kingsTileDrawPoint);
            yield return new WaitUntil(() => updatedDrawPoint);
            photonView.RPC(nameof(BanBlessing), RpcTarget.All); //�Ȃ���ƓV�a, �n�a���ł��Ȃ��Ȃ�.
            yield return new WaitUntil(() => !checker.canBlessings);
            endDraw = true;
        }
        else
        {
            updatedDrawPoint = false;
            finalTileWinPoint = 0;
            kingsTileDrawPoint = 0;
            photonView.RPC(nameof(UpdateDrawPoint), RpcTarget.All, nowTurn,
                finalTileWinPoint, kingsTileDrawPoint);
            yield return new WaitUntil(() => updatedDrawPoint);
            endDraw = true;
        }
        yield return new WaitUntil(() => endDraw);

        yield return StartCoroutine(DoraCount(nowTurn));
    }

    IEnumerator DoraCount(int order)
    {
        if(order != myOrder)
        {
            people[order].countedDora = false;
            people[order].DoraCountOfHand(doraList);
            yield return new WaitUntil(() => people[order].countedDora);
        }
        if(discarded)
        {
            updatedDrawPoint = false;
            int finalTileWinPoint = 0;
            int kingsTileDrawPoint = 0;
            photonView.RPC(nameof(UpdateDrawPoint), RpcTarget.All, nowTurn,
                finalTileWinPoint, kingsTileDrawPoint);
            yield return new WaitUntil(() => updatedDrawPoint);
        }
        changedUI = false;
        photonView.RPC(nameof(UpdateUIOfAllPlayer), RpcTarget.All);
        yield return new WaitUntil(() => changedUI);
    }

    //[PunRPC]�͊֐��𓯊����邽�߂̂���.
    //�I�u�W�F�N�g����ɓ������Ă���킯�ł͂Ȃ��̂�, ���\�b�h�̎��s��ϐ��̕ύX���s�������ꍇ��,
    //���̃X�N���v�g���擾���鏈�����s��, ���s����K�v������.
    [PunRPC]
    public void UpdateUIOfAllPlayer()
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.people[gameManager.myOrder].countedDora = false;
        gameManager.people[gameManager.myOrder].DoraCountOfHand(gameManager.doraList);
        StartCoroutine(WaitCountingDora());
    }

    //�h���𐔂��Đ����𖞗p��UI���X�V����.
    public IEnumerator WaitCountingDora()
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        yield return new WaitUntil(() => gameManager.people[gameManager.myOrder].countedDora);
        UIController uiController = gameController.GetComponent<UIController>();
        uiController.UpdateUI(gameManager.wallTiles.Count);
        changedUI = true;
    }

    IEnumerator JudgeBlessing()
    {
        bool endJudge = false;
        people[nowTurn].draw = false;
        people[nowTurn].judged = false;
        if (drawCount < 4 && checker.canBlessings)
        {
            didPlusDrawCount = false;
            photonView.RPC(nameof(PlusDrawCount), RpcTarget.All);
            yield return new WaitUntil(() => didPlusDrawCount);

            //�V�ۏ���.
            if (checker.CanBlessing(people[nowTurn].hand,
                people[nowTurn].mainHandArray) && drawCount == 1)
            {
                if (people[nowTurn].isHuman == true)
                {
                    uiController.buttonType = UIController.BUTTON_TYPE.DRAW;
                    uiController.FetchButton();
                }
                else
                {
                    yield return StartCoroutine(uiController.OnDrawEffect(nowTurn));
                }
                //�V�a���邩�ǂ����҂�.
                yield return new WaitUntil(() => people[nowTurn].judged);
                if (people[nowTurn].draw == true)
                {
                    //���U���g�����̏���.
                    wasGameSet = false;
                    uiController.yakuType = UIController.YAKU_TYPE.BLESSING_OF_HEAVEN;
                    photonView.RPC(nameof(SendResultInfo), RpcTarget.All, nowTurn, uiController.yakuType);
                    yield return new WaitUntil(() => wasGameSet);
                    yield return StartCoroutine(tileController.WinMove(myOrder, nowTurn));

                    endJudge = true;
                }
                else
                {
                    endJudge = true;
                }
            }
            //�n�ۏ���.
            else if (checker.CanBlessing(people[nowTurn].hand,
                people[nowTurn].mainHandArray) && drawCount != 1)
            {
                if (people[nowTurn].isHuman == true)
                {
                    uiController.buttonType = UIController.BUTTON_TYPE.DRAW;
                    uiController.FetchButton();
                }
                else
                {
                    yield return StartCoroutine(uiController.OnDrawEffect(nowTurn));
                }
                //�n�a���邩�ǂ����҂�.
                yield return new WaitUntil(() => people[nowTurn].judged);
                if (people[nowTurn].draw == true)
                {
                    //���U���g�����̏���.
                    wasGameSet = false;
                    uiController.yakuType = UIController.YAKU_TYPE.BLESSING_OF_EARTH;
                    photonView.RPC(nameof(SendResultInfo), RpcTarget.All, nowTurn, uiController.yakuType);
                    yield return new WaitUntil(() => wasGameSet);
                    yield return StartCoroutine(tileController.WinMove(myOrder, nowTurn));

                    endJudge = true;
                }
                else
                {
                    endJudge = true;
                }
            }
            else
            {
                endJudge = true;
            }
        }
        else
        {
            endJudge = true;
        }
        yield return new WaitUntil(() => endJudge);
    }

    IEnumerator JudgeDrawFourQuads()
    {
        bool endJudge = false;
        people[nowTurn].draw = false;
        people[nowTurn].judged = false;
        //�c���l�Ȏq����.
        if (checker.CanDrawFourQuads(people[nowTurn].hand))
        {
            if (people[nowTurn].isHuman == true)
            {
                uiController.buttonType = UIController.BUTTON_TYPE.DRAW;
                uiController.FetchButton();
            }
            else
            {
                yield return StartCoroutine(uiController.OnDrawEffect(nowTurn));
            }
            //�l�Ȏq���邩�ǂ����҂�.
            yield return new WaitUntil(() => people[nowTurn].judged);
            if (people[nowTurn].draw == true)
            {
                //���U���g�����̏���.
                wasGameSet = false;
                uiController.yakuType = UIController.YAKU_TYPE.FOUR_QUADS;
                photonView.RPC(nameof(SendResultInfo), RpcTarget.All, nowTurn, uiController.yakuType);
                yield return new WaitUntil(() => wasGameSet);
                yield return StartCoroutine(tileController.WinMove(myOrder, nowTurn));

                endJudge = true;
            }
            else
            {
                endJudge = true;
            }
        }
        else
        {
            endJudge = true;
        }
        
        yield return new WaitUntil(() => endJudge);
    }

    IEnumerator JudgeCountingGrandSlum()
    {
        bool endJudge = false;
        people[nowTurn].draw = false;
        people[nowTurn].judged = false;
        //�����𖞏���.
        if (checker.CanCountingGrandSlum(people[nowTurn].hand, people[nowTurn].mainHandArray, 
                people[nowTurn].kongCount, people[nowTurn].meldedKongCount,�@people[nowTurn].doraCount,
                        people[nowTurn].finalTileWin, people[nowTurn].kingsTileDraw))
        {
            if (people[nowTurn].isHuman == true)
            {
                uiController.buttonType = UIController.BUTTON_TYPE.DRAW;
                uiController.FetchButton();
            }
            else
            {
                yield return StartCoroutine(uiController.OnDrawEffect(nowTurn));
            }
            //�����𖞂��邩�ǂ����҂�.
            yield return new WaitUntil(() => people[nowTurn].judged);
            if (people[nowTurn].draw == true)
            {
                //���U���g�����̏���.
                people[nowTurn].countedDora = false;
                wasGameSet = false;
                uiController.yakuType = UIController.YAKU_TYPE.COUNTING_GRAND_SLUM;
                photonView.RPC(nameof(SendResultInfo), RpcTarget.All, nowTurn, uiController.yakuType);
                yield return new WaitUntil(() => people[nowTurn].countedDora);
                yield return new WaitUntil(() => wasGameSet);
                yield return StartCoroutine(tileController.WinMove(myOrder, nowTurn));

                endJudge = true;
            }
            else
            {
                endJudge = true;
            }
        }
        else
        {
            endJudge = true;
        }
        yield return new WaitUntil(() => endJudge);
    }

    IEnumerator JudgeConcealedKong()
    {
        bool endJudge = false;
        while(checker.CanConcealedKong(wallTiles.Count, 
            people[nowTurn].hand, people[nowTurn].mainHandArray))
        {
            bool endConcealedKong = false;
            ChangeDidConcealedKong(false, false);
            if (people[nowTurn].isHuman == true)
            {
                uiController.buttonType = UIController.BUTTON_TYPE.CONCEALED_KONG;
                uiController.FetchButton();
            }
            else
            {
                people[nowTurn].didConcealedKong = true;
                people[nowTurn].judged = true;
            }
            //�ÞȂ��邩�ǂ����҂�.
            yield return new WaitUntil(() => people[nowTurn].judged);
            if (people[nowTurn].didConcealedKong == true)
            {
                selectedConcealedKong = false;
                if (checker.canConcealedKongCounter == 1)
                {
                    concealedKongTileNum = checker.canConcealedKongTileNum[0];
                    selectedConcealedKong = true;
                }
                else
                {
                    if (people[nowTurn].isHuman == true)
                    {
                        //�ǂ���̞Ȃ����邩�I��������.
                        uiController.ChangeSelectConcealedImages
                            (checker.canConcealedKongTileNum[0], 
                            checker.canConcealedKongTileNum[1]);

                        uiController.selectConcealedKongTile.SetActive(true);
                    }
                    else
                    {
                        int randomIndex = Random.Range(0, checker.canConcealedKongTileNum.Count);
                        concealedKongTileNum = checker.canConcealedKongTileNum[randomIndex];
                        selectedConcealedKong = true;
                    }
                }
                yield return new WaitUntil(() => selectedConcealedKong);
                yield return StartCoroutine(uiController.OnConcealedKongEffect(nowTurn));

                yield return StartCoroutine(tileController.ConcealedKong(nowTurn, concealedKongTileNum));
                yield return StartCoroutine(SortingAfterDiscarded());
                if(doraCounter < tileController.doraIndicators.Length)
                {
                    yield return StartCoroutine(AddingDora());
                }

                yield return StartCoroutine(DrawWallTile(wallTiles.Count - 1, false));
                yield return StartCoroutine(JudgeDrawFourQuads());
                if (isGameSet)
                {
                    endJudge = true;
                    break;
                }
                yield return StartCoroutine(JudgeCountingGrandSlum());
                if (isGameSet)
                {
                    endJudge = true;
                    break;
                }
                endConcealedKong = true;
            }
            else
            {
                endJudge = true;
                break;
            }
            yield return new WaitUntil(() => endConcealedKong);
        }
        if(!checker.CanConcealedKong(wallTiles.Count,
            people[nowTurn].hand, people[nowTurn].mainHandArray))
        {
            endJudge = true;
        }
        yield return new WaitUntil(() => endJudge);
    }

    //�U�蒮(= ������(��v�ŌǗ����Ă���)�v�������Ŋ��ɉ͂Ɏ̂ĂĂ��ă����ł��Ȃ����)Checker.
    IEnumerator JudgeFuritenn()
    {
        bool furitenn = checker.Furitenn(people[nowTurn].hand,
            people[nowTurn].mainHandArray, people[nowTurn].insideWall);

        changedFuritennState = false;
        photonView.RPC(nameof(ChangeFuritennState), RpcTarget.All, nowTurn, furitenn);
        yield return new WaitUntil(() => changedFuritennState);
    }

    //3�������Ă��Ȃ��v�̒����烉���_���Ŕv���̂Ă�.
    //�@�B, �����w�K��œK�헪���Ƃ邱�ƂȂǍs�����Ƃ��l������, �Q�[���̃R���Z�v�g�I��,
    //���S�҂��V�ԑz��Ȃ̂Ń����_���Ƃ����ȈՓI�Ȃ��̂��ǂ��Ɣ��f��, �s��Ȃ�����.
    IEnumerator CPUDiscardTile()
    {
        GameObject container = null;
        switch (nowTurn)
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
        bool checkCanDiscard = false;
        List<int> canDiscardIndexList = new List<int>();
        HandContainer handContainer = container.GetComponent<HandContainer>();
        foreach (HandContainerChild handContainerChild in handContainer)
        {
            if (handContainerChild.JudgeDiscard())
            {
                canDiscardIndexList.Add(handContainerChild.handIndex);
            }
            if (handContainerChild.handIndex == handContainer.Count - 1)
            {
                checkCanDiscard = true;
            }
        }
        yield return new WaitUntil(() => checkCanDiscard);
        int randomIndex = Random.Range(0, canDiscardIndexList.Count);
        int discardIndex = canDiscardIndexList[randomIndex];
        yield return new WaitForSeconds(0.5f);  //�̂Ă��v�̈ʒu�𑼂̃v���C���[�����f���邽�߂̎���.
        handContainer[discardIndex].DiscardTile(nowTurn);
    }

    IEnumerator JudgeRonFourQuads()
    {
        bool endJudge = false;
        for (int i = 1; i < numPlayers; i++)
        {
            bool endCheck = false;
            int ronOrder = (nowTurn + i) % numPlayers;
            people[ronOrder].ron = false;
            people[ronOrder].judged = false;
            if (checker.CanRonFourQuads(people[ronOrder].hand,
                people[ronOrder].furitenn, lastDiscardedTileNum))
            {
                if (people[ronOrder].isHuman == true)
                {
                    photonView.RPC(nameof(FetchRonButton), RpcTarget.All, ronOrder);
                }
                else
                {
                    yield return StartCoroutine(uiController.OnRonEffect(ronOrder));
                }
            }
            else
            {
                people[ronOrder].judged = true;
            }
            //�������邩�ǂ����҂�.
            yield return new WaitUntil(() => people[ronOrder].judged);
            if (people[ronOrder].ron == true)
            {
                wasGameSet = false;
                uiController.lastDiscardedTileNum = lastDiscardedTileNum;
                uiController.yakuType = UIController.YAKU_TYPE.FOUR_QUADS;
                photonView.RPC(nameof(SendResultInfo), RpcTarget.All, ronOrder, uiController.yakuType);
                yield return new WaitUntil(() => wasGameSet);
                yield return StartCoroutine(tileController.WinMove(myOrder, ronOrder));
                endJudge = true;
                break;
            }
            else
            {
                //�����ł���̂�, �����Ɍ�������1�������U�蒮�ɂȂ�.
                if (checker.CanRonFourQuads(people[ronOrder].hand,
                    people[ronOrder].furitenn, lastDiscardedTileNum))
                {
                    bool furitenn = true;
                    changedFuritennState = false;
                    photonView.RPC(nameof(ChangeFuritennState), RpcTarget.All, ronOrder, furitenn);
                    yield return new WaitUntil(() => changedFuritennState);
                }
                endCheck = true;
            }
            yield return new WaitUntil(() => endCheck);
            if(i == numPlayers - 1)
            {
                endJudge = true;
            }
        }
        yield return new WaitUntil(() => endJudge);
    }

    IEnumerator JudgeMeldedKong()
    {
        bool endJudge = false;
        for (int i = 1; i < numPlayers; i++)
        {
            bool endCheck = false;
            int meldedKongOrder = (nowTurn + i) % numPlayers;
            yield return StartCoroutine(WaitChangingDidMeldedKong
                (meldedKongOrder, false, false));

            if (checker.CanMeldedKong(wallTiles.Count, 
                people[meldedKongOrder].mainHandArray, lastDiscardedTileNum))
            {
                if (people[meldedKongOrder].isHuman == true)
                {
                    photonView.RPC(nameof(FetchMeldedKongButton), RpcTarget.All, meldedKongOrder);
                }
                else
                {
                    yield return StartCoroutine(uiController.OnMeldedKongEffect(meldedKongOrder));
                }
            }
            else
            {
                yield return StartCoroutine(WaitChangingDidMeldedKong
                    (meldedKongOrder, false, true));
            }
            //�喾�Ȃ��邩�ǂ����҂�.
            yield return new WaitUntil(() => people[meldedKongOrder].judged);
            if (people[meldedKongOrder].didMeldedKong == true)
            {
                yield return StartCoroutine(tileController.MeldedKong(meldedKongOrder, 
                    nowTurn, lastDiscardedTileNum));

                endJudge = true;
                break;
            }
            else
            {
                endCheck = true;
            }
            yield return new WaitUntil(() => endCheck);
            endJudge = true;
        }
        yield return new WaitUntil(() => endJudge);
    }

    IEnumerator AddingDora()
    {
        yield return new WaitForSeconds(0.5f);
        photonView.RPC(nameof(ReadyToAddDora), RpcTarget.All);
        DoraFripper doraFripper = doraContainer[doraCounter].GetComponent<DoraFripper>();
        nextReady = false;
        doraFripper.InitialSetUpOfDora(doraCounter, nowTurn, people[nowTurn].isHuman);
        yield return new WaitUntil(() => nextReady);
    }

    IEnumerator WaitChangingDidMeldedKong(int order, bool didMeldedKong, bool judged)
    {
        changedDidMeldedKong = false;
        photonView.RPC(nameof(ChangeDidMeldedKong), RpcTarget.All, order, didMeldedKong, judged);
        yield return new WaitUntil(() => changedDidMeldedKong);
    }

    IEnumerator SortingAfterDiscarded()
    {
        yield return new WaitForSeconds(0.5f);
        tileController.sortedHand = false;
        photonView.RPC(nameof(tileController.SortHand), RpcTarget.All, nowTurn);
        yield return new WaitUntil(() => tileController.sortedHand);
    }

    IEnumerator AdvanceTurn()
    {
        bool endTurn = false;
        if(wallTiles.Count > 0)
        {
            bool meldedKongOrderExist = false;
            int meldedKongOrder = -1;
            for (int i = 1; i < numPlayers; i++)
            {
                int checkOrder = (nowTurn + i) % numPlayers;
                if (people[checkOrder].didMeldedKong)
                {
                    meldedKongOrderExist = true;
                    meldedKongOrder = checkOrder;
                    break;
                }
            }
            if (meldedKongOrderExist)
            {
                wentNextTurn = false;
                photonView.RPC(nameof(GoNextTurn), RpcTarget.All, meldedKongOrder - 1);
                yield return new WaitUntil(() => wentNextTurn);
            }
            else
            {
                wentNextTurn = false;
                photonView.RPC(nameof(GoNextTurn), RpcTarget.All, nowTurn);
                yield return new WaitUntil(() => wentNextTurn);
            }
            endTurn = true;
        }
        else
        {
            //�v�R�̎c�肪0�ŒN���a����Ȃ������ꍇ, ���v�����C���ō\�����ꂽ�v���C���[�̏����ɂȂ�.
            for(int i = 0; i < numPlayers; i++)
            {
                if (people[i].type == 3)
                {
                    winnerOrder = i;
                }
            }
            wasGameSet = false;
            uiController.yakuType = UIController.YAKU_TYPE.FLOWING_YAKUMANN;
            photonView.RPC(nameof(SendResultInfo), RpcTarget.All, winnerOrder, uiController.yakuType);
            yield return new WaitUntil(() => wasGameSet);
            endTurn = true;
        }
        yield return new WaitUntil(() => endTurn);
    }

    //[PunRPC]�͊֐��𓯊����邽�߂̂���.
    //�I�u�W�F�N�g����ɓ������Ă���킯�ł͂Ȃ��̂�, �֐��̎��s��ϐ��̕ύX�𓯊�����ꍇ��,
    //����X�N���v�g���擾����, ���̌�Ɏ��s����K�v������.
    [PunRPC]
    public void RemoveWallTiles(int drawIndex)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.wallTiles.RemoveAt(drawIndex);
        removedWallTiles = true;
    }

    [PunRPC]
    private void UpdateDrawPoint(int order, int finalTileWinPoint, int kingsTileDrawPoint)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.people[order].finalTileWin = finalTileWinPoint;
        gameManager.people[order].kingsTileDraw = kingsTileDrawPoint;
        updatedDrawPoint = true;
    }

    [PunRPC]
    public void PlusDrawCount()
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.drawCount++;
        gameManager.didPlusDrawCount = true;
    }

    [PunRPC]
    public void BanBlessing()
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        Checker checker = gameController.GetComponent<Checker>();
        checker.canBlessings = false;
    }

    private void ChangeDidConcealedKong(bool didConcealedKong, bool judged)
    {
        people[nowTurn].didConcealedKong = didConcealedKong;
        people[nowTurn].judged = judged;
    }

    [PunRPC]
    public void ChangeFuritennState(int order, bool furitenn)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.people[order].furitenn = furitenn;
        if(order == gameManager.myOrder)
        {
            UIController uiController = gameController.GetComponent<UIController>();
            uiController.furitennImage.SetActive(furitenn);
        }
        gameManager.changedFuritennState = true;
    }

    [PunRPC]
    public void FetchRonButton(int ronOrder)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        UIController uiController = gameController.GetComponent<UIController>();
        if (ronOrder == uiController.myOrder)
        {
            uiController.buttonType = UIController.BUTTON_TYPE.RON;
            uiController.FetchButton();
        }
    }

    [PunRPC]
    public void FetchMeldedKongButton(int meldedKongOrder)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        UIController uiController = gameController.GetComponent<UIController>();
        if (meldedKongOrder == uiController.myOrder)
        {
            uiController.buttonType = UIController.BUTTON_TYPE.MELDED_KONG;
            uiController.FetchButton();
        }
    }

    [PunRPC]
    public void ChangeDidMeldedKong(int order, bool didMeldedKong, bool judged)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.people[order].didMeldedKong = didMeldedKong;
        gameManager.people[order].judged = judged;
        gameManager.changedDidMeldedKong = true;
    }

    [PunRPC]
    public void ReadyToAddDora()
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        UIController uiController = gameController.GetComponent<UIController>();
        uiController.Playing_Panel.SetActive(false);
        uiController.Dora_Panel.SetActive(true);
        GameManager gameManager = gameController.GetComponent<GameManager>();
        CameraController cameraController = gameController.GetComponent<CameraController>();
        cameraController.DoraCameraTransform(gameManager.doraCounter);
    }

    [PunRPC]
    public void GoNextTurn(int turnNum)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.nowTurn = (turnNum +  1) % numPlayers;
        gameManager.wentNextTurn = true;
    }

    [PunRPC]
    public void SendResultInfo(int winnerOrder, UIController.YAKU_TYPE yakuType)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        UIController uiController = gameController.GetComponent<UIController>();
        uiController.yakuType = yakuType;

        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.winnerOrder = winnerOrder;

        if (uiController.yakuType == UIController.YAKU_TYPE.COUNTING_GRAND_SLUM)
        {
            //13�͂�𑗂�.
            gameManager.people[gameManager.winnerOrder].DoraCountOfHand(gameManager.doraList);           
        }

        gameManager.isGameSet = true;
        gameManager.wasGameSet = true;
    }

    //�ق��v���C���[�̃Q�[���̋����I���΍��p.
    //���ꂪ�Ȃ��Ɣv�̃I�u�W�F�N�g�������ăo�O��.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        uiController.isClick = true;
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(3);
        photonView.RPC(nameof(uiController.ReportDestroyGame), RpcTarget.All);

        StartCoroutine(BackLobby());
    }

    //�Q�[���I���ネ�r�[�ɖ߂邽�߂̃��\�b�h.
    IEnumerator BackLobby()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            yield return new WaitForSeconds(3f);
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }
}
