using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;    //�ύX(�ύX�͎Q�l�ɂ������r�[�쐬�X�N���v�g����ύX�����ӏ�������).

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public SoundManager soundManager;
    [Header("Connection Status")]
    //�ύX.
    public TextMeshProUGUI connectionStatusText;    //�T�[�o�[�ڑ���.

    [Header("Login UI Panel")]
    //�ύX.
    public TMP_InputField playerNameInput;
    public GameObject Login_UI_Panel;

    [Header("Game Options UI Panel")]
    public GameObject GameOptions_UI_Panel; //���[���쐬�E�Q���I���p�l��.

    [Header("Create Room UI Panel")]
    public GameObject CreateRoom_UI_Panel; 
    //�ύX.
    public TMP_InputField roomNameInputField;  
    //�ύX.
    public TMP_InputField maxPlayerInputField; 

    [Header("Inside Room UI Panel")]
    public GameObject InsideRoom_UI_Panel; 
    //�ύX.
    public TextMeshProUGUI roomInfoText;   //���[�����E�v���C�l���̃e�L�X�g.
    public GameObject playerListPrefab; //�������Ă���v���C���[������Prefab.
    public GameObject playerListContent;    //playerListPrefab������GameObject.
    public GameObject startGameButton;  

    [Header("Room List UI Panel")]
    public GameObject RoomList_UI_Panel;    
    public GameObject roomListEntryPrefab;  //�쐬���ꂽ���[��������Prefab.
    public GameObject roomListParentGameObject; //roomListEntryPrefab������GameObject.

    [Header("Join Random Room UI Panel")]
    public GameObject JoinRandomRoom_UI_Panel;  //�����_���v���C�I����̉�ʂ�����Panel.

    //�ύX.
    private int min_maxPlayerInputField = 1;    //�ő�v���C�l��1�`4�l.
    private int max_maxPlayerInputField = 4;

    private Dictionary<string, RoomInfo> cachedRoomList;    //�擾�������[�����E�v���C�l��.
    private Dictionary<string, GameObject> roomListGameObjects; //�擾�������[��.
    private Dictionary<int, GameObject> playerListGameObjects;  //�擾�������[���ɂ���v���C���[�l��.
    

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameObjects = new Dictionary<string, GameObject>();
        PhotonNetwork.AutomaticallySyncScene = true;
        //�}�X�^�[�N���C�A���g�̃V�[���ύX���ق��̃N���C�A���g�Ɏ������f�����Ă���.

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
        //�ύX.
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
    
    //���[���쐬.
    public void OnCreateRoomButtonClicked()
    {
        //�ύX.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);
        //�ύX.
        //�ő�v���C�l����Unity���Ő����ꌅ���͂Ɛݒ肵�Ă���, ���̐��������͂ł���悤�ɂȂ�.
        //���̂��� -n �� - �݂̂����͉ɂȂ�.
        //������͂������߂̏���.
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

        //�ύX.
        //�ő�v���C�l����1-4�łȂ��������͂���.
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
    
    //�I�v�V�����I����ʂɖ߂�.
    public void OnCancelButtonClicked()
    {
        //�ύX.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(2);

        ActivatePanel(GameOptions_UI_Panel.name);
    }
    
    public void OnGoToCreateRoomButtonClicked()
    {
        //�ύX.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);

        ActivatePanel(CreateRoom_UI_Panel.name);
    } 

    //���r�[�ɓ��胋�[���ꗗ��ʂɈړ�.
    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        //�ύX.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);

        ActivatePanel(RoomList_UI_Panel.name);
    }

    //���r�[���o�ăI�v�V�����I����ʂɈړ�.
    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        //�ύX.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(2);

        ActivatePanel(GameOptions_UI_Panel.name);
    }
    
    public void OnLeaveGameButtonClicked()
    {
        //�ύX.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(2);

        PhotonNetwork.LeaveRoom();
    }
    
    //�����_���}�b�`��ʂɈړ�.
    public void OnJoinRandomRoomButtonClicked()
    {
        //�ύX.
        soundManager.soundType = SoundManager.SOUND_TYPE.BUTTON;
        soundManager.PlaySoundEffect(1);

        ActivatePanel(JoinRandomRoom_UI_Panel.name);
        PhotonNetwork.JoinRandomRoom();
    }
    
    //GameScene�Ɉړ����A�Q�[���X�^�[�g.
    public void OnStartGameButtonClicked()
    {
        //�ύX.
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
    //�l�b�g�ڑ����ꂽ�Ƃ�.
    public override void OnConnected()
    {
        //Debug.Log("Connected to Internet");
    }

    //Photon�T�[�o�[�֐ڑ�������I�v�V�����I����ʂɈړ�.
    public override void OnConnectedToMaster()
    {
        //Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to Photon");
        ActivatePanel(GameOptions_UI_Panel.name);

    }

    public override void OnCreatedRoom()
    {
        //Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created.");
    }
    
    //���[���Q�����Ƀv���C���[���X�g�̃I�u�W�F�N�g���X�V����.
    public override void OnJoinedRoom()
    {
        //Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(InsideRoom_UI_Panel.name);
        //���[���̃z�X�g�̂݃Q�[�����J�n�ł���悤�ɂ���.
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }

        roomInfoText.text = "���[�����F" + PhotonNetwork.CurrentRoom.Name + "\n" + "�Q���l���F" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        //�v���C���[���X�g�̃I�u�W�F�N�g�X�V.
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerListGameObject = Instantiate(playerListPrefab);
            playerListGameObject.transform.SetParent(playerListContent.transform);
            playerListGameObject.transform.localScale = Vector3.one;

            playerListGameObject.transform.Find("PlayerNameText").GetComponent<TextMeshProUGUI>().text = player.NickName;
            //ActorNumber��(�l�b�g���[�N���)���[�����ɓ������v���C���[���ꂼ��Ɋ��蓖�Ă���ID.
            //LocalPlayer�̓��[�J���v���C = �Q�[�����v���C���Ă���{�l ������.
            //�������O���ʂ��邽�ߎ����̖��O�̉��ɂ�"��(PlayerIndicator)"�ƕ\������.
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

    //���[���������Ƀv���C���[���X�g�̃I�u�W�F�N�g���X�V����.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomInfoText.text = "���[�����F" + PhotonNetwork.CurrentRoom.Name + "\n" + "�Q���l���F" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

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

    //�ق��v���C���[�����[���ގ����v���C���[���X�g�̃I�u�W�F�N�g���X�V����.
    //�܂��A�z�X�g�ɂȂ�΃Q�[���J�n�{�^�����o������.
    //�Ƃ����̂�, �z�X�g�̂݃Q�[���J�n�ł���悤�ɂ��Ă��邩��.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomInfoText.text = "���[�����F" + PhotonNetwork.CurrentRoom.Name + "\n" + "�Q���l���F" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

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

    //���r�[�������ɎQ���ł��郋�[����\������.
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

        //�Q���ł��郋�[���Ƃ��̃��[���ɓ��邽�߂̃{�^���̕\��.
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
    
    //�����ł��郉���_���}�b�`���O�p���[�����Ȃ���΍쐬����.
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //Debug.Log(message);

        string roomName = "Room " + Random.Range(1000, 10000);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = max_maxPlayerInputField;   //�ύX.

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    
    #endregion


    #region Private Methods
    //���[������.
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
    //��ʐ؂�ւ�.
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