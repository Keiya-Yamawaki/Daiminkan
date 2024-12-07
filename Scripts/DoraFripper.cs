using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DoraFripper : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] AudioClip fripTileSE;
    [SerializeField] AudioClip dropTileSE;

    //orderが一致するかどうかで捲れるか変わる.そのために初めは一致しないように-2としている.
    public int orderOfFripper = -1;
    public int myOrder = -2;    
    public int doraIndex;
    public int tileNum;
    public bool isHuman = false;
    public HandContainerChild handContainerChild;
    public GameObject gameController;
    public GameManager gameManager;
    public UIController uiController;
    public bool onText = true;

    private float tileLength = 0.28f;   //牌の縦幅.
    //private float tileWidth = 0.21f;   //牌の横幅.
    //private float tileHeight = 0.165f;   //牌の厚み.
    private float tileRatio = 1000.0f;   //牌をスクリーンで見たときの大きさに合わせるための倍率.
    private float screenWidthPercentage = 1f / 3f;   //スクリーン幅に対する割合.
    private float screenHeightPercentage = 4f / 5f;  //スクリーン高さに対する割合.

    public bool isScrolled = true;
    public bool isFinished = false;    //牌をめくり終えたかどうか.
    public bool inside = false;      //マウスカーソルがスクリーンの指定範囲内にあるかどうか.

    private float flightTime = 2 * 1.0f / 5.0f;
    private float rotationDegrees = 180f;    //回転させる角度.
    private float liftHeight = 9.8f / (2.0f * 5.0f * 5.0f);     //上昇させる高さ.

    private float timer = -1.0f;
    private Vector3 lastMousePosition;  //直前のマウスの位置.

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindWithTag("GameController");
        gameManager = gameController.GetComponent<GameManager>();
        uiController = gameController.GetComponent<UIController>();
        handContainerChild = GetComponent<HandContainerChild>();
        audioSource = GetComponent<AudioSource>();
        myOrder = gameManager.myOrder;
        doraIndex = handContainerChild.doraIndex;
        tileNum = handContainerChild.tileNum;
    }

    // Update is called once per frame
    void Update()
    {
        //ほかプレイヤーのドラ捲りを待つテキストの表示.
        if (!isScrolled && !onText && (orderOfFripper != myOrder))
        {
            uiController.waitText.SetActive(true);
            onText = true;
        }

        //CPUが槓した場合は自動でドラをめくる.
        if (!isScrolled & !isHuman)
        {
            isScrolled = true;
            StartCoroutine(CPUDoraFrip());
        }

        //自分がめくるとき.
        if (!isScrolled && isHuman && (orderOfFripper == myOrder))
        {
            if (!onText)
            {
                uiController.fingerImage.SetActive(true);
                uiController.swipeText.SetActive(true);
                onText = true;
            }

            //範囲内から出ずにドラッグされた牌をめくる.
            Detection();
            if (inside && Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
            }
            if (!isScrolled && lastMousePosition.y > 0 && Input.GetMouseButton(0))
            {
                //下方向に牌半個分以上移動していれば牌を回転させる.
                Vector3 deltaMousePosition = Input.mousePosition - lastMousePosition;
                if (deltaMousePosition.y < - tileLength * tileRatio / 2) 
                {
                    isScrolled = true;
                    uiController.fingerImage.SetActive(false);   //アニメーションの変更.
                    uiController.swipeText.SetActive(false);
                    //牌上昇速度を全プレイヤー画面で与え, ひっくり返す.
                    Vector3 velocity = new Vector3(0, Mathf.Sqrt(2.0f * 9.8f * liftHeight), 0); 
                    PhotonView photonView = GetComponent<PhotonView>();
                    photonView.RPC(nameof(ReverseDora), RpcTarget.All, doraIndex, velocity);
                }
            }
        }

        if (timer >= 0)
        {
            if(timer == 0)
            {
                audioSource.PlayOneShot(fripTileSE);
            }
            this.timer += Time.deltaTime;
            float rate = timer / flightTime;
            if (rate >= 1)
            {
                rate = 1;
                timer = -1.0f;
            }
            //牌を半回転させる.
            float angle = Mathf.Lerp(0f, rotationDegrees, rate);
            this.transform.rotation = Quaternion.Euler(angle, 180.0f, 90.0f);
            if (rate == 1)
            {
                audioSource.PlayOneShot(dropTileSE);
                Vector3 velocity = Vector3.zero;
                this.GetComponent<Rigidbody>().velocity = velocity;
                this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                isFinished = true;
            }
        }

        //牌をめくり終えたらテキストを消し, ドラを追加し, もとのプレイ画面に戻る.
        if (isFinished)
        {
            isFinished = false;
            uiController.waitText.SetActive(false);
            uiController.fingerImage.SetActive(false);
            uiController.swipeText.SetActive(false);
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(NextReady());
            }
        }
    }

    //マウスカーソルがスクリーンの指定範囲内にあるか確認するメソッド.
    public void Detection()
    {
        //スクリーンの真ん中1/3、下2/3の範囲.
        Rect detectionArea = new Rect(Screen.width * (1 - screenWidthPercentage) / 2, 0, Screen.width * screenWidthPercentage, Screen.height * screenHeightPercentage);

        if (detectionArea.Contains(Input.mousePosition))
        {
            inside = true;
        }
        else
        {
            inside = false;
            lastMousePosition.y = 0;
            //左クリックしたまま指定範囲外に出ると, 画面上の方まで引っ張って下ろした時に牌がめくれる.
            //この時牌の上をなぞっている感じでなくなるため, これをなくす様加えた処理.
        }
    }

    IEnumerator CPUDoraFrip()
    {
        yield return new WaitForSeconds(0.5f);  //この時間だと人がめくっているようなタイミングに見える.
        Vector3 velocity = new Vector3(0, Mathf.Sqrt(2.0f * 9.8f * liftHeight), 0);
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(ReverseDora), RpcTarget.All, doraIndex, velocity);
    }

    //ドラを追加し, もとのプレイ画面に戻る.
    IEnumerator NextReady()
    {
        yield return new WaitForSeconds(1f);
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(AddDora), RpcTarget.All);
    }

    public void InitialSetUpOfDora(int doraIndex, int orderOfFripper, bool isHuman)
    {
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(InitialSetUpToFrip), RpcTarget.All, doraIndex, orderOfFripper, isHuman);
    }

    //[PunRPC]は関数を同期できる様にするためのもの.
    //オブジェクトを常に同期しているわけではないので,
    //毎回スクリプトを取得してからそのスクリプトのメソッドを実行する.

    //ドラ牌をめくれるようにY座標とX軸の固定を外したり, 捲る人の順番(order)を指定する.
    [PunRPC]
    public void InitialSetUpToFrip(int doraIndex, int orderOfFripper, bool isHuman)
    {
        GameObject container = null;
        HandContainer doraContainer = null;
        DoraFripper doraFripper = null;
        container = GameObject.FindWithTag("DoraContainer");
        doraContainer = container.GetComponent<HandContainer>();
        doraFripper = doraContainer[doraIndex].GetComponent<DoraFripper>();
        Rigidbody rb = doraFripper.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX
            | RigidbodyConstraints.FreezePositionZ
            | RigidbodyConstraints.FreezeRotationY
            | RigidbodyConstraints.FreezeRotationZ;
        doraFripper.orderOfFripper = orderOfFripper;
        doraFripper.onText = false;
        doraFripper.isHuman = isHuman;
        doraFripper.isScrolled = false;
    }

    [PunRPC]
    public void ReverseDora(int doraIndex, Vector3 velocity)
    {
        GameObject container = null;
        HandContainer doraContainer = null;
        DoraFripper doraFripper = null;
        container = GameObject.FindWithTag("DoraContainer");
        doraContainer = container.GetComponent<HandContainer>();
        doraFripper = doraContainer[doraIndex].GetComponent<DoraFripper>();
        doraFripper.isScrolled = true;
        doraFripper.orderOfFripper = -1;
        doraFripper.isHuman = false;
        doraFripper.GetComponent<Rigidbody>().velocity = velocity;
        doraFripper.timer = 0.0f;  //タイマースタート.
        //doraFripper.GetComponent<Rigidbody>().velocity = velocity;
    }

    //次の種類の牌をドラとして加え, 元のプレイ画面に戻るようにカメラを移動させ, パネル変更する.
    //ドラは次の数字の牌、数牌の最後、風牌、三元牌は同種の最初の牌になる.
    [PunRPC]
    public void AddDora()
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        switch (tileNum)
        {
            case 0:
                gameManager.doraList.Add(6);
                break;
            case 4:
                gameManager.doraList.Add(0);
                gameManager.doraList.Add(5);
                break;
            case 9:
                gameManager.doraList.Add(1);
                break;
            case 10:
                gameManager.doraList.Add(16);
                break;
            case 14:
                gameManager.doraList.Add(10);
                gameManager.doraList.Add(15);
                break;
            case 19:
                gameManager.doraList.Add(11);
                break;
            case 20:
                gameManager.doraList.Add(26);
                break;
            case 24:
                gameManager.doraList.Add(20);
                gameManager.doraList.Add(25);
                break;
            case 29:
                gameManager.doraList.Add(21);
                break;
            case 33:
                gameManager.doraList.Add(30);
                break;
            case 36:
                gameManager.doraList.Add(34);
                break;
            default:
                gameManager.doraList.Add(tileNum + 1);
                break;
        }
        UIController uiController = gameController.GetComponent<UIController>();
        uiController.changeDoraIndicator(gameManager.doraCounter, tileNum);
        gameManager.doraCounter++;
        CameraController cameraController = gameController.GetComponent<CameraController>();
        cameraController.PlayingCameraTransform(gameManager.myOrder);
        uiController.Dora_Panel.SetActive(false);
        uiController.Playing_Panel.SetActive(true);
        gameManager.nextReady = true;
    }
}
