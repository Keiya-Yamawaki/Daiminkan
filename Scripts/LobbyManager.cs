using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;    //変更(変更は参考にしたロビー作成スクリプトから変更した箇所を示す).

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public SoundManager soundManager;
    [Header("Connection Status")]
    //変更.
    public TextMeshProUGUI connectionStatusText;    //サーバー接続状況.

    [Header("Login UI Panel")]
    //変更.
    public TMP_InputField playerNameInput;
    public GameObject Login_UI_Panel;

    [Header("Game Options UI Panel")]
    public GameObject GameOptions_UI_Panel; //ルーム作成・参加選択パネル.

    [Header("Create Room UI Panel")]
    public GameObject CreateRoom_UI_Panel; 
    //変更.
    public TMP_InputField roomNameInputField;  
    //変更.
    public TMP_InputField maxPlayerInputField; 

    [Header("Inside Room UI Panel")]
    public GameObject InsideRoom_UI_Panel; 
    //変更.
    public TextMeshProUGUI roomInfoText;   //ルーム名・プレイ人数のテキスト.
    public GameObject playerListPrefab; //入室しているプレイヤーを示すPrefab.
    public GameObject playerListContent;    //playerListPrefabを入れるGameObject.
    public GameObject startGameButton;  

    [Header("Room List UI Panel")]
    public GameObject RoomList_UI_Panel;    
    public GameObject roomListEntryPrefab;  //作成されたルームを示すPrefab.
    public GameObject roomListParentGameObject; //roomListEntryPrefabを入れるGameObject.

    [Header("Join Random Room UI Panel")]
    public GameObject JoinRandomRoom_UI_Panel;  //ランダムプレイ選択後の画面を示すPanel.

    //変更.
    private int min_maxPlayerInputField = 1;    //最大プレイ人数1～4人.
    private int max_maxPlayerInputField = 4;

    private Dictionary<string, RoomInfo> cachedRoomList;    //取得したルーム名・プレイ人数.
    private Dictionary<string, GameObject> roomListGameObjects; //取得したルーム.
    private Dictionary<int, GameObject> playerListGameObjects;  //取得したルームにいるプレイヤー人数.
    

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameObjects = new Dictionary<string, GameObject>();
        PhotonNetwork.AutomaticallySyncScene = true;
        //マスタークライアントのシーン変更をほかのクライアントに自動反映させている.

        if (!PhotonNetwork.InRoom)
        {
            ActivatePanel(Login_UI_Panel.name);
        }
        else
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        connectionStatusText.text = "Connection status: " + PhotonNetwork.NetworkClientState;
    }

    #endregion


    #region UI Callbacks
    public void OnLoginButtonClicked()
    {
        //変更.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);
        string playerName = playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            //Debug.Log("PlayerName is invalid!");
        }

    }
    
    //ルーム作成.
    public void OnCreateRoomButtonClicked()
    {
        //変更.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);
        //変更.
        //最大プレイ人数をUnity側で整数一桁入力と設定しており, 負の整数も入力できるようになる.
        //そのため -n の - のみも入力可になる.
        //これをはじくための処理.
        if (string.IsNullOrEmpty(maxPlayerInputField.text)
            || maxPlayerInputField.text == "-")
        {
            return; 
        }

        string roomName = roomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();

        //変更.
        //最大プレイ人数が1-4でない数字をはじく.
        if (min_maxPlayerInputField <= int.Parse(maxPlayerInputField.text) 
            && int.Parse(maxPlayerInputField.text) <= max_maxPlayerInputField)
        {
            roomOptions.MaxPlayers = (byte)int.Parse(maxPlayerInputField.text);
        }
        else
        {
            return;
        }

        PhotonNetwork.CreateRoom(roomName, roomOptions);

    }
    
    //オプション選択画面に戻る.
    public void OnCancelButtonClicked()
    {
        //変更.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(2);

        ActivatePanel(GameOptions_UI_Panel.name);
    }
    
    public void OnGoToCreateRoomButtonClicked()
    {
        //変更.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);

        ActivatePanel(CreateRoom_UI_Panel.name);
    } 

    //ロビーに入りルーム一覧画面に移動.
    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        //変更.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);

        ActivatePanel(RoomList_UI_Panel.name);
    }

    //ロビーを出てオプション選択画面に移動.
    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        //変更.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(2);

        ActivatePanel(GameOptions_UI_Panel.name);
    }
    
    public void OnLeaveGameButtonClicked()
    {
        //変更.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(2);

        PhotonNetwork.LeaveRoom();
    }
    
    //ランダムマッチ画面に移動.
    public void OnJoinRandomRoomButtonClicked()
    {
        //変更.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);

        ActivatePanel(JoinRandomRoom_UI_Panel.name);
        PhotonNetwork.JoinRandomRoom();
    }
    
    //GameSceneに移動し、ゲームスタート.
    public void OnStartGameButtonClicked()
    {
        //変更.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
    
    
    #endregion

    #region Photon Callbacks
    //ネット接続されたとき.
    public override void OnConnected()
    {
        //Debug.Log("Connected to Internet");
    }

    //Photonサーバーへ接続したらオプション選択画面に移動.
    public override void OnConnectedToMaster()
    {
        //Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to Photon");
        ActivatePanel(GameOptions_UI_Panel.name);

    }

    public override void OnCreatedRoom()
    {
        //Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created.");
    }
    
    //ルーム参加時にプレイヤーリストのオブジェクトを更新する.
    public override void OnJoinedRoom()
    {
        //Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(InsideRoom_UI_Panel.name);
        //ルームのホストのみゲームを開始できるようにする.
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }

        roomInfoText.text = "ルーム名：" + PhotonNetwork.CurrentRoom.Name + "\n" + "参加人数：" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        //プレイヤーリストのオブジェクト更新.
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerListGameObject = Instantiate(playerListPrefab);
            playerListGameObject.transform.SetParent(playerListContent.transform);
            playerListGameObject.transform.localScale = Vector3.one;

            playerListGameObject.transform.Find("PlayerNameText").GetComponent<TextMeshProUGUI>().text = player.NickName;
            //ActorNumberは(ネットワーク上の)ルーム内に入ったプレイヤーそれぞれに割り当てられるID.
            //LocalPlayerはローカルプレイ = ゲームをプレイしている本人 を示す.
            //似た名前特別するため自分の名前の横には"私(PlayerIndicator)"と表示する.
            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
            }
            else
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);

            }
            playerListGameObjects.Add(player.ActorNumber, playerListGameObject);
        }
    }

    //ルーム入室時にプレイヤーリストのオブジェクトを更新する.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomInfoText.text = "ルーム名：" + PhotonNetwork.CurrentRoom.Name + "\n" + "参加人数：" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        GameObject playerListGameObject = Instantiate(playerListPrefab);
        playerListGameObject.transform.SetParent(playerListContent.transform);
        playerListGameObject.transform.localScale = Vector3.one;

        playerListGameObject.transform.Find("PlayerNameText").GetComponent<TextMeshProUGUI>().text = newPlayer.NickName;
        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
        }
        else
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
        }
        playerListGameObjects.Add(newPlayer.ActorNumber, playerListGameObject);
    }

    //ほかプレイヤーがルーム退室時プレイヤーリストのオブジェクトを更新する.
    //また、ホストになればゲーム開始ボタンが出現する.
    //というのも, ホストのみゲーム開始できるようにしているから.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomInfoText.text = "ルーム名：" + PhotonNetwork.CurrentRoom.Name + "\n" + "参加人数：" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        if (playerListGameObjects != null && playerListGameObjects.ContainsKey(otherPlayer.ActorNumber))
        {
            Destroy(playerListGameObjects[otherPlayer.ActorNumber].gameObject);
            playerListGameObjects.Remove(otherPlayer.ActorNumber);
        }

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
    }

    public override void OnLeftRoom()
    {
        ActivatePanel(GameOptions_UI_Panel.name);

        if (playerListGameObjects != null)
        {
            foreach (GameObject playerListGameObject in playerListGameObjects.Values)
            {
                Destroy(playerListGameObject);
            }
            playerListGameObjects.Clear();
            playerListGameObjects = null;
        }
    }

    //ロビー入室時に参加できるルームを表示する.
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        foreach (RoomInfo room in roomList)
        {
            //Debug.Log(room.Name);
            if (!room.IsOpen || !room.IsVisible || room.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList.Remove(room.Name);
                }
            }
            else
            {
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList[room.Name] = room;
                }
                else
                {
                    cachedRoomList.Add(room.Name, room);
                }
            }
        }

        //参加できるルームとそのルームに入るためのボタンの表示.
        foreach (RoomInfo room in cachedRoomList.Values)
        {
            GameObject roomListEntryGameObject = Instantiate(roomListEntryPrefab);
            roomListEntryGameObject.transform.SetParent(roomListParentGameObject.transform);
            roomListEntryGameObject.transform.localScale = Vector3.one;

            roomListEntryGameObject.transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>().text = room.Name;
            roomListEntryGameObject.transform.Find("RoomPlayersText").GetComponent<TextMeshProUGUI>().text = room.PlayerCount + " / " + room.MaxPlayers;
            roomListEntryGameObject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinRoomButtonClicked(room.Name));
            
            roomListGameObjects.Add(room.Name, roomListEntryGameObject);
        }
    }
    
    public override void OnLeftLobby()
    {
        ClearRoomListView();
        cachedRoomList.Clear();
    }
    
    //入室できるランダムマッチング用ルームがなければ作成する.
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //Debug.Log(message);

        string roomName = "Room " + Random.Range(1000, 10000);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = max_maxPlayerInputField;   //変更.

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    
    #endregion


    #region Private Methods
    //ルーム入室.
    void OnJoinRoomButtonClicked(string _roomName)
    {
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(_roomName);

    }
    
    void ClearRoomListView()
    {
        foreach (var roomListGameObject in roomListGameObjects.Values)
        {
            Destroy(roomListGameObject);
        }

        roomListGameObjects.Clear();
    }
    #endregion

    #region  Public Methods
    //画面切り替え.
    public void ActivatePanel(string panelToBeActivated)
    {
        Login_UI_Panel.SetActive(panelToBeActivated.Equals(Login_UI_Panel.name));
        GameOptions_UI_Panel.SetActive(panelToBeActivated.Equals(GameOptions_UI_Panel.name));
        CreateRoom_UI_Panel.SetActive(panelToBeActivated.Equals(CreateRoom_UI_Panel.name));
        InsideRoom_UI_Panel.SetActive(panelToBeActivated.Equals(InsideRoom_UI_Panel.name));
        RoomList_UI_Panel.SetActive(panelToBeActivated.Equals(RoomList_UI_Panel.name));
        JoinRandomRoom_UI_Panel.SetActive(panelToBeActivated.Equals(JoinRandomRoom_UI_Panel.name));

    }
    #endregion

}