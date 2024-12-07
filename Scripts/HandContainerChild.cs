using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class HandContainerChild : MonoBehaviourPunCallbacks
{
    //このスクリプトがアタッチされたオブジェクトの所有者をネットワーク上での所有者とする.
    //これはNPCのオブジェクトをマスタークライアントが処理する場合のことを考えている.
    public Player Owner => photonView.Owner;    
    public int order = -1;
    public int myOrder = -2;
    public int handIndex = -1;
    public int doraIndex = -1;
    public int tileNum = -1;
    public int verticalDirection = 1;   //槓したときに縦に置く(0なら横).
    public string tileName = null;
    GameObject basicTable;  //麻雀台.
    GameObject gameController;
    GameManager gameManager;
    UIController uiController;
    SoundManager soundManager;
    Vector3 mousePosition;
    Ray ray;
    RaycastHit hit;
    private Vector3 liftHand = Vector3.zero;
    public Vector3 initialHandPosition = Vector3.zero;
    public bool myTurnEnd;
    public bool isWaited;
    public bool canLift;
    public bool canDiscard;

    // Start is called before the first frame update
    void Start()
    {
        //orderと一致しているかどうかで自分の牌か判断する処理がこの後あるので
        //-1～3以外の数字を使っている(-1はorderの初期条件).
        myOrder = -2;  
        basicTable = GameObject.FindWithTag("BasicTable");
        gameController = GameObject.FindWithTag("GameController");
        gameManager = gameController.GetComponent<GameManager>();
        uiController = gameController.GetComponent<UIController>();
        soundManager = gameController.GetComponent<SoundManager>();
        myOrder = gameManager.myOrder;
        TiltHandTile();
    }

    // Update is called once per frame
    void Update()
    {
        //手牌の牌にカーソルを合わせると持ち上がる処理.
        if (order == myOrder && isWaited)
        {
            LiftHandTile();
        }

        //自分の番に手牌からいらない牌を選択して捨て, それ以外の牌を捨てられないようにする処理.
        if (order == myOrder && !myTurnEnd && canDiscard)
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject && Input.GetMouseButtonDown(0))
            {
                MyTurnEnd();
                uiController.NotActiveSetUp();
                DiscardTile(myOrder);
            }
        }
        
        //3枚持っている牌にカーソルを合わせると, 捨てられないと表示する.
        if (order == myOrder && !myTurnEnd && !canDiscard && !canLift)
        {
            if (!uiController.cantDiscardText.activeSelf)
            {
                uiController.cantDiscardText.SetActive(true);
            }
        }
    }

    //各々の手牌は麻雀台に立てておかれている.
    //そのままでは自分の手牌が見えないので画面に対して平行になるように手牌を傾ける.
    private void TiltHandTile()
    {
        if (order == myOrder)
        {
            switch (order)
            {
                case 0:
                    this.transform.rotation = TileController.myHandQuaternion0;
                    liftHand = TileController.liftHand0;
                    break;
                case 1:
                    this.transform.rotation = TileController.myHandQuaternion1;
                    liftHand = TileController.liftHand1;
                    break;
                case 2:
                    this.transform.rotation = TileController.myHandQuaternion2;
                    liftHand = TileController.liftHand2;
                    break;
                case 3:
                    this.transform.rotation = TileController.myHandQuaternion3;
                    liftHand = TileController.liftHand3;
                    break;
                default:
                    break;
            }
        }
    }

    //待っている間も楽しめるように、手牌の牌にカーソルを合わせると牌を持ち上げられるようにしている.
    //また持ち上げた際に気持ち良い音が鳴るようにしている.
    private void LiftHandTile()
    {
        mousePosition = Input.mousePosition;
        ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == gameObject && canLift)
            {
                initialHandPosition = this.transform.position;
                this.transform.position += liftHand;
                canLift = false;
                soundManager.soundType = SoundManager.SOUND_TYPE.MYTILE;
                soundManager.PlaySoundEffect(0);
                JudgeDiscard();
            }
            //牌か麻雀台にカーソルがあっていなければ牌を元の高さにもどす.
            else if (hit.collider.gameObject != gameObject && hit.collider.gameObject != basicTable && !canLift) 
            {
                this.transform.position = initialHandPosition;
                canLift = true;
                canDiscard = false;
                uiController.cantDiscardText.SetActive(false);
            }
        }
    }


    /*
     * 同じ牌を3個持っていたら捨てられないルールなので, 3個持っているか判断するメソッド.
     * また、赤ドラを判断するために数字でなく別で割り当てた名前で判断している.
     * (数字を割り当てた理由は赤ドラと普通の牌を区別するため.)
     * 例：萬子の5と萬子の赤ドラの5は A5, A5R で前2文字が一致しているから同じ牌と判断する.
     * それぞれの名前は,
     * 萬子：A 数字 (赤ドラなら最後にRがつく), 筒子：B 数字, 索子：C 数字, 字牌：H 数字.
     * としている.
    */
    public bool JudgeDiscard()
    {
        canDiscard = false;
        GameObject container = null;
        container = searchHandContainer(order);
        HandContainer handContainer = container.GetComponent<HandContainer>();
        string tempTileName = name.Substring(0, 2);
        int sameTileCount = handContainer.handGOList.Count(tile => tile.tileName.Substring(0, 2) == tempTileName);
        if(sameTileCount != 3)
        {
            canDiscard = true;
        }
        return canDiscard;
    }

    //手牌を捨てた後などに手牌を捨てたり,
    //持ち上げた後にソートされたときに初期位置が変更されないようにするためのクールタイム.
    public IEnumerator LiftHandCoolTime()
    {
        isWaited = false;
        canLift = true;
        canDiscard = false;
        yield return new WaitForSeconds(0.5f);
        isWaited = true;
    }

    //手牌を捨てた後などに手牌を捨てられないようにするための処理.
    private void MyTurnEnd()
    {
        GameObject container = null;
        container = searchHandContainer(myOrder);

        HandContainer handContainer = container.GetComponent<HandContainer>();
        foreach(var handContainerChild in handContainer.handGOList)
        {
            handContainerChild.myTurnEnd = true;
        }
    }

    private GameObject searchHandContainer(int order)
    {
        GameObject container = null;
        switch (order)
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
        return container;
    }

    public void DiscardTile(int ownerOrder)
    {
        //牌を手牌として認識し, 持ち上げられないように所有者なしにする.
        order = -1;
        canDiscard = false;
        isWaited = false;
        canLift = true;

        //河の親オブジェクトを取得.
        GameObject container = null;
        switch (ownerOrder)
        {
            case 0:
                container = GameObject.FindWithTag("InsideWall0");
                break;
            case 1:
                container = GameObject.FindWithTag("InsideWall1");
                break;
            case 2:
                container = GameObject.FindWithTag("InsideWall2");
                break;
            case 3:
                container = GameObject.FindWithTag("InsideWall3");
                break;
            default:
                break;
        }

        //河は6列で構成され, 左から敷き詰めて置く.
        HandContainer handContainer = container.GetComponent<HandContainer>();
        int HandContainerCount_horizon = handContainer.Count % 6;
        int HandContainerCount_vertical = handContainer.Count / 6;
        //置き場と向きの設定.
        Vector3 dicardedPosition = Vector3.zero;
        Quaternion insideWallQuaternion = Quaternion.identity;
        switch (ownerOrder)
        {
            case 0:
                dicardedPosition = TileController.insideWallPosition0
                    + HandContainerCount_horizon * TileController.horizonInsideWallOffset0 
                    + HandContainerCount_vertical * TileController.verticalInsideWallOffset0;
                insideWallQuaternion = TileController.insideWallQuaternion0;
                break;
            case 1:
                dicardedPosition = TileController.insideWallPosition1
                    + HandContainerCount_horizon * TileController.horizonInsideWallOffset1
                    + HandContainerCount_vertical * TileController.verticalInsideWallOffset1;
                insideWallQuaternion = TileController.insideWallQuaternion1;
                break;
            case 2:
                dicardedPosition = TileController.insideWallPosition2
                    + HandContainerCount_horizon * TileController.horizonInsideWallOffset2
                    + HandContainerCount_vertical * TileController.verticalInsideWallOffset2;
                insideWallQuaternion = TileController.insideWallQuaternion2;
                break;
            case 3:
                dicardedPosition = TileController.insideWallPosition3
                    + HandContainerCount_horizon * TileController.horizonInsideWallOffset3
                    + HandContainerCount_vertical * TileController.verticalInsideWallOffset3;
                insideWallQuaternion = TileController.insideWallQuaternion3;
                break;
            default:
                break;
        }

        //牌を捨てるのを同期.
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(Discard), RpcTarget.All, ownerOrder, handIndex, dicardedPosition, insideWallQuaternion);

        gameManager.discarded = true;
        soundManager.soundType = SoundManager.SOUND_TYPE.TILE;
        soundManager.PlaySoundEffect(2);
    }

    //[PunRPC]は関数を同期するためのもの.
    [PunRPC]
    public void Discard(int ownerOrder, int handIndex, Vector3 dicardedPosition, Quaternion insideWallQuaternion)
    {
        //手牌と河の親オブジェクトを取得し, 捨て牌を移動させ, 親オブジェクトを変更する.
        GameObject firstContainer = null;
        GameObject secondContainer = null;
        switch (ownerOrder)
        {
            case 0:
                firstContainer = GameObject.FindWithTag("Person0");
                secondContainer = GameObject.FindWithTag("InsideWall0");
                break;
            case 1:
                firstContainer = GameObject.FindWithTag("Person1");
                secondContainer = GameObject.FindWithTag("InsideWall1");
                break;
            case 2:
                firstContainer = GameObject.FindWithTag("Person2");
                secondContainer = GameObject.FindWithTag("InsideWall2");
                break;
            case 3:
                firstContainer = GameObject.FindWithTag("Person3");
                secondContainer = GameObject.FindWithTag("InsideWall3");
                break;
            default:
                break;
        }

        //最後に捨てた牌の情報の更新(大明槓やロンできるか判定時に使う).
        HandContainer handContainer = firstContainer.GetComponent<HandContainer>();
        handContainer[handIndex].verticalDirection = 0;     //槓されたとき, 横向きに置くために0にする.
        GameObject discardedTile = handContainer[handIndex].gameObject;
        int lastDiscardedTileNum = handContainer[handIndex].tileNum;

        //捨て牌を河の子オブジェクトとして河に捨てる.
        discardedTile.transform.parent = null;
        discardedTile.transform.rotation = insideWallQuaternion;
        discardedTile.transform.position = dicardedPosition;
        if (secondContainer != null)
        {
            discardedTile.transform.SetParent(secondContainer.transform);
        }

        //手牌と河の情報の更新.
        gameController = GameObject.FindWithTag("GameController");
        gameManager = gameController.GetComponent<GameManager>();
        gameManager.lastDiscardedTileNum = lastDiscardedTileNum;
        List<int> ownerHand = gameManager.people[ownerOrder].hand;
        ownerHand.Remove(lastDiscardedTileNum);
        List<int> ownerInsideWall = gameManager.people[ownerOrder].insideWall;
        ownerInsideWall.Add(lastDiscardedTileNum);
        gameManager.lastDiscardedTile = discardedTile;  //最後に捨てた牌の情報の更新.
        gameManager.people[gameManager.nowTurn].judged = true;  //trueにして暗槓やツモボタンを消す.
    }

}
