using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using ExitGames.Client.Photon;


/*
 * 注意!!!
 * 同期が重要なターン性ゲームなのでコルーチンで同期が完了するまで待機するのを多々使用している.
 * 例えば, 手牌の同期の処理とソートの同期の処理がほぼ同時に始まると,
 * 手牌の同期とソートの同期が並列処理され, 同期される情報が間違ったものになる.
 * 手牌の1つ目の牌を同期する間にソートされ, 2つ目の牌が1つ目の牌と同じ牌なのに同期されるなどが起こる.
 * そのため同期するものがある場合は完全に処理が終わるのをまってから, 次の処理を行う必要がある.
*/


//IPunTurnManagerCallbacksで簡単にターン性ゲームの実装ができるが,
//CPUの処理も同時にできるやり方が調べても(できるかどうか)わからなかったため.
//使用せずに, 自分で実装することに.
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
    private const string NamesKey = "n"; //Keyの文字数が多いと同期の処理が重くなるので1文字を扱っている.
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
    private ExitGames.Client.Photon.Hashtable typesHashtable;   //牌の種類. 1：萬子, 2：筒子, 3：索子, 4：字牌の情報.
    private ExitGames.Client.Photon.Hashtable mainHandsHashtable;   //槓できる同じ3つの牌の数字4組の情報.
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
    public List<int> wallTiles;     //牌山(≒山札).
    public List<int> deadWall;      //王牌(ドラ表示牌).
    public List<int> doraList;      //公開したドラ表示牌.
    public WallTilesModel wallTilesModel;
    public CameraController cameraController;
    public PeopleModel peopleModel;
    public List<Person> people;
    public TileController tileController;
    public HandContainer doraContainer;
    public SoundManager soundManager;
    #endregion

    //手牌やカメラなどの初期設定を行う.
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

        //ネットワーク上で変数や関数を同期するためのスクリプト.
        PhotonView photonView = GetComponent<PhotonView>(); 
        //手牌や牌山, 王牌などの作成をマスタークライアントが行う.
        //手牌等をそれぞれのプレイヤーが行うとその度に同期する必要があるためホストがすべて行う.
        //例えば牌山の情報が更新される前に牌を引いてしまい, 同じ牌を引くバグが発生する可能性がある.
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            wallTilesModel = GetComponent<WallTilesModel>();
            wallTilesModel.MakeWallTiles(numPlayers);
            wallTiles = wallTilesModel.wallTiles;
            deadWall = wallTilesModel.deadWall;
            DecidePeapleInfo(players);
            PhotonNetwork.CurrentRoom.SetCustomProperties(allHashtables);   
            //Hashtable(変数やリスト)情報を送信している.
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
        orders = Shuffle(initialList);  //プレイ順.
        types = Shuffle(initialList);   //主な手牌の種類.
        createHashtables(players);
        //ActorNumberはネットワーク上のID. CPUを扱えないので name や id.
        //手牌などの情報を自作する.
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
        //ActorNumberはネットワーク上のID. CPUを扱えないので name や id.
        //手牌などの情報を自作する.
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
        //CPUの情報追加.
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

        //各 Hashtable を allHashtables に格納.
        //この時 Keyの文字数が多いと同期の処理が重くなるので1文字を扱っている.
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

    //Hashtable 更新時 Hashtable をコピーする.
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

    //Hashtable のコピーが済むとローカルプレイヤーや牌山などの情報を設定する.
    //また, 手牌などのプレイヤー毎の情報を格納するためにプレイヤーオブジェクトを作成する.
    //これも, ネットワーク上でプレイヤーのオブジェクトは存在するがCPU用が存在しないから作成する.
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
        //初期のセットアップの処理を待たないと手牌を傾ける処理が遅れて行われ.
        //スペックの低いPCで参加すると他人の手牌が見えるバグが発生する.
        //それを回避するために短時間処理を待つ.
        yield return new WaitForSeconds(0.5f);
        //牌毎にネットワーク上のidを割り当てていくため大きめの数字(千の位)で所有者を区別する.
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
             * 関数を同期するためのコード. 第一引数が同期する関数. 
             * 第二引数は全員か自分以外かで実装することを選択することができる.
             * 自分以外の場合別で自分の処理が必要なのと, 
             * なぜかうまく処理されずバグが発生するので全員で実行している.
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
            //マスタークライアントは自身とCPUの手牌オブジェクトを出現させる.
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

    //[PunRPC]は関数を同期するためのもの.
    //オブジェクトを常に同期しているわけではないのでメソッドの実行や変数の変更等を行いたい場合は.
    //スクリプトを取得してから実行する必要がある.
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
    public bool discarded = false;  //手牌を捨てたか.
    public bool removedWallTiles = false; 
    public bool updatedDrawPoint = false;   //嶺上開花と海底撈月を更新済みか.
    public bool didPlusDrawCount = false;   //牌を引いた回数の合計を計算済みかどうか.
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
    //天和, 地和は槓されずに最初の引いた牌で上がれた時のみなので4までカウントする.
    public int drawCount = 0;   
    public int lastDiscardedTileNum = -1;
    public GameObject lastDiscardedTile = null;
    public Checker checker;     //役判定スクリプト.
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
        //リザルト処理. その後ロビーに戻る.
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
        yield return new WaitUntil(() => !PhotonNetwork.InRoom); // Photon ルームから退出するのを待機.
        SceneManager.LoadScene(sceneName); 
    }

    IEnumerator PlayTurn()
    {
        discarded = false;
        if (!people[nowTurn].didMeldedKong)
        {
            //通常時は牌山の最初から引く.
            yield return StartCoroutine(DrawWallTile(0, true));
            yield return StartCoroutine(JudgeBlessing());
            if (isGameSet)
            {
                yield break;
            }
        }
        else
        {
            //槓したときは牌山の最後から引く.
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

        //捨て牌入力待ち.
        //CPUは自動で手牌から1つ選んで捨てさせる.
        if (!(people[nowTurn].isHuman))
        {
            yield return StartCoroutine(CPUDiscardTile());
        }
        yield return new WaitUntil(() => discarded);

        yield return StartCoroutine(JudgeFuritenn());
        yield return StartCoroutine(SortingAfterDiscarded());

        //14回目まで大明槓後にドラをめくり, 全部プレイヤーのドラを数える.
        if (people[nowTurn].didMeldedKong && doraCounter < tileController.doraIndicators.Length)
        {
            yield return StartCoroutine(AddingDora());
        }
        yield return StartCoroutine(DoraCount(nowTurn));

        yield return StartCoroutine(WaitChangingDidMeldedKong
            (nowTurn, false, false));

        //大明槓よりロンが優先されるのでロンできるかどうかの処理を先に行う.
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

        int finalTileWinPoint;  //海底撈月(牌山最後の牌で上がった時につく役).
        int kingsTileDrawPoint; //嶺上開花(槓して引いたときに上がれると付く役).
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
            photonView.RPC(nameof(BanBlessing), RpcTarget.All); //槓すると天和, 地和ができなくなる.
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

    //[PunRPC]は関数を同期するためのもの.
    //オブジェクトを常に同期しているわけではないので, メソッドの実行や変数の変更を行いたい場合は,
    //そのスクリプトを取得する処理を行い, 実行する必要がある.
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

    //ドラを数えて数え役満用のUIを更新する.
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

            //天保処理.
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
                //天和するかどうか待ち.
                yield return new WaitUntil(() => people[nowTurn].judged);
                if (people[nowTurn].draw == true)
                {
                    //リザルト処理の準備.
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
            //地保処理.
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
                //地和するかどうか待ち.
                yield return new WaitUntil(() => people[nowTurn].judged);
                if (people[nowTurn].draw == true)
                {
                    //リザルト処理の準備.
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
        //ツモ四槓子処理.
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
            //四槓子するかどうか待ち.
            yield return new WaitUntil(() => people[nowTurn].judged);
            if (people[nowTurn].draw == true)
            {
                //リザルト処理の準備.
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
        //数え役満処理.
        if (checker.CanCountingGrandSlum(people[nowTurn].hand, people[nowTurn].mainHandArray, 
                people[nowTurn].kongCount, people[nowTurn].meldedKongCount,　people[nowTurn].doraCount,
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
            //数え役満するかどうか待ち.
            yield return new WaitUntil(() => people[nowTurn].judged);
            if (people[nowTurn].draw == true)
            {
                //リザルト処理の準備.
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
            //暗槓するかどうか待ち.
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
                        //どちらの槓をするか選択させる.
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

    //振り聴(= あたり(手牌で孤立している)牌を自分で既に河に捨てていてロンできない状態)Checker.
    IEnumerator JudgeFuritenn()
    {
        bool furitenn = checker.Furitenn(people[nowTurn].hand,
            people[nowTurn].mainHandArray, people[nowTurn].insideWall);

        changedFuritennState = false;
        photonView.RPC(nameof(ChangeFuritennState), RpcTarget.All, nowTurn, furitenn);
        yield return new WaitUntil(() => changedFuritennState);
    }

    //3枚持っていない牌の中からランダムで牌を捨てる.
    //機械, 強化学習や最適戦略をとることなど行うことも考えたが, ゲームのコンセプト的に,
    //初心者も遊ぶ想定なのでランダムという簡易的なものが良いと判断し, 行わなかった.
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
        yield return new WaitForSeconds(0.5f);  //捨てた牌の位置を他のプレイヤーが判断するための時間.
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
            //ロンするかどうか待ち.
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
                //ロンできるのに, せずに見逃すと1順だけ振り聴になる.
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
            //大明槓するかどうか待ち.
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
            //牌山の残りが0で誰も和了れなかった場合, 字牌がメインで構成されたプレイヤーの勝利になる.
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

    //[PunRPC]は関数を同期するためのもの.
    //オブジェクトを常に同期しているわけではないので, 関数の実行や変数の変更を同期する場合は,
    //毎回スクリプトを取得して, その後に実行する必要がある.
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
            //13はんを送る.
            gameManager.people[gameManager.winnerOrder].DoraCountOfHand(gameManager.doraList);           
        }

        gameManager.isGameSet = true;
        gameManager.wasGameSet = true;
    }

    //ほかプレイヤーのゲームの強制終了対策用.
    //これがないと牌のオブジェクトが消えてバグる.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        uiController.isClick = true;
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(3);
        photonView.RPC(nameof(uiController.ReportDestroyGame), RpcTarget.All);

        StartCoroutine(BackLobby());
    }

    //ゲーム終了後ロビーに戻るためのメソッド.
    IEnumerator BackLobby()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            yield return new WaitForSeconds(3f);
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }
}
