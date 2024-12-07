using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;


/*
 * 注意!!!
 * 同期が重要なターン性ゲームなのでコルーチンで同期が完了するまで待機するのを多々使用している.
 * 例えば, 手牌の同期の処理とソートの同期の処理がほぼ同時に始まると,
 * 手牌の同期とソートの同期が並列処理され, 同期される情報が間違ったものになる.
 * 手牌の1つ目の牌を同期する間にソートされ, 2つ目の牌が1つ目の牌と同じ牌なのに同期されるなどが起こる.
 * そのため同期するものがある場合は完全に処理が終わるのをまってから, 次の処理を行う必要がある.
*/

public class TileController : MonoBehaviourPunCallbacks
{
    HandContainerChild handContainerChild;
    public SoundManager soundManager;
    public GameObject[] tiles;
    public Sprite[] texturesOfDoraIndicator;    //ドラ表示.
    public GameObject[] doraIndicators;         //ドラ表示牌.
    public bool instantiatedDora = false;
    public bool instantiatedHand = false;
    public bool sortedHand = false;
    public bool easySortedHand = false;
    public bool tiltedAllHands = false;
    public bool endKong = false;
    private bool wetherConcealedKong;
    private int kongCount = 4;
    public int copyKongCount = 0;
    public int viewId = 1000;   //ネットワークオブジェクトの個別ID. 千の位で所有者を判断する.
    private Vector3 InstantiateHandPosition = Vector3.zero;
    private Quaternion handQuaternion = Quaternion.identity;
    private Vector3 kongPosition = Vector3.zero;
    private Vector3 kongOffset = Vector3.zero;
    private Vector3 horizonKongOffset = Vector3.zero;
    private Vector3 verticalKongOffset = Vector3.zero;
    private Quaternion kongQuaternion = Quaternion.identity;
    private Quaternion anotherKongQuaternion = Quaternion.identity;

    [Header("HandTransform")]
    public static Vector3 handPosition0 = new Vector3(-2f, 0.14f, -2.75f);
    public static Vector3 handPosition1 = new Vector3(2.75f, 0.14f, -2f);
    public static Vector3 handPosition2 = new Vector3(2f, 0.14f, 2.75f);
    public static Vector3 handPosition3 = new Vector3(-2.75f, 0.14f, 2f);
    public static Vector3 handOffset0 = new Vector3(0.215f, 0f, 0f);
    public static Vector3 handOffset1 = new Vector3(0f, 0f, 0.215f);
    public static Vector3 handOffset2 = new Vector3(-0.215f, 0f, 0f);
    public static Vector3 handOffset3 = new Vector3(0f, 0f, -0.215f);
    //カーソルを合わせたときに持ち上げる方向.
    public static Vector3 liftHand0 = new Vector3(0f, 0.05f, 0.0866f);  
    public static Vector3 liftHand1 = new Vector3(-0.0866f, 0.05f, 0f);
    public static Vector3 liftHand2 = new Vector3(0f, 0.05f, -0.0866f);
    public static Vector3 liftHand3 = new Vector3(0.0866f, 0.05f, 0f);
    public static Quaternion handQuaternion0 = Quaternion.Euler(-90f, 0f, -90f);
    public static Quaternion handQuaternion1 = Quaternion.Euler(-90f, -90f, -90f);
    public static Quaternion handQuaternion2 = Quaternion.Euler(-90f, 180f, -90f);
    public static Quaternion handQuaternion3 = Quaternion.Euler(-90f, 90f, -90f);
    //麻雀台に垂直に置くとなんの牌かわからないので画面と並行になるように傾ける.
    public static Quaternion myHandQuaternion0 = Quaternion.Euler(-30f, 0f, -90f);
    public static Quaternion myHandQuaternion1 = Quaternion.Euler(-150f, 90f, -270f);
    public static Quaternion myHandQuaternion2 = Quaternion.Euler(-150f, 0f, 90f);
    public static Quaternion myHandQuaternion3 = Quaternion.Euler(-150f, -90f, 90f);

    [Header("InsideWallTransform")]
    public static Vector3 insideWallPosition0 = new Vector3(-0.6f, 0.09f, -1f);
    public static Vector3 insideWallPosition1 = new Vector3(1f, 0.09f, -0.6f);
    public static Vector3 insideWallPosition2 = new Vector3(0.6f, 0.09f, 1f);
    public static Vector3 insideWallPosition3 = new Vector3(-1f, 0.09f, 0.6f);
    public static Vector3 horizonInsideWallOffset0 = new Vector3(0.24f, 0f, 0f);
    public static Vector3 horizonInsideWallOffset1 = new Vector3(0f, 0f, 0.24f);
    public static Vector3 horizonInsideWallOffset2 = new Vector3(-0.24f, 0f, 0f);
    public static Vector3 horizonInsideWallOffset3 = new Vector3(0f, 0f, -0.24f);
    public static Vector3 verticalInsideWallOffset0 = new Vector3(0f, 0f, -0.3f);
    public static Vector3 verticalInsideWallOffset1 = new Vector3(0.3f, 0f, 0f);
    public static Vector3 verticalInsideWallOffset2 = new Vector3(0f, 0f, 0.3f);
    public static Vector3 verticalInsideWallOffset3 = new Vector3(-0.3f, 0f, 0f);
    public static Quaternion insideWallQuaternion0 = Quaternion.Euler(0f, 0f, -90f);
    public static Quaternion insideWallQuaternion1 = Quaternion.Euler(0f, -90f, -90f);
    public static Quaternion insideWallQuaternion2 = Quaternion.Euler(0f, 180f, -90f);
    public static Quaternion insideWallQuaternion3 = Quaternion.Euler(0f, 90f, -90f);

    //暗槓したとき, 自分の牌4枚を表2枚と裏2枚を縦向きに倒しておく.
    //他人の牌で槓した場合は槓した他人の席の位置に合わせて他人の牌を横向きに,
    //自分の牌を縦向きに置く.
    //右側の人なら4枚のうち最も右, 左側の人なら最も左, 対面なら左から2番目に他人の牌を置く.
    //そのためのオフセット.
    [Header("KongTransform")]
    public static Vector3 kongPosition0 = new Vector3(2.5f, 0.09f, -2.75f);
    public static Vector3 kongPosition1 = new Vector3(2.75f, 0.09f, 2.5f);
    public static Vector3 kongPosition2 = new Vector3(-2.5f, 0.09f, 2.75f);
    public static Vector3 kongPosition3 = new Vector3(-2.75f, 0.09f, -2.5f);
    public static Vector3 basicKongOffset0 = new Vector3(-0.215f, 0f, 0f);
    public static Vector3 basicKongOffset1 = new Vector3(0f, 0f, -0.215f);
    public static Vector3 basicKongOffset2 = new Vector3(0.215f, 0f, 0f);
    public static Vector3 basicKongOffset3 = new Vector3(0f, 0f, 0.215f);

    public static Vector3 horizonKongOffset0 = new Vector3(-0.035f, 0f, 0f);
    public static Vector3 horizonKongOffset1 = new Vector3(0f, 0f, -0.035f);
    public static Vector3 horizonKongOffset2 = new Vector3(0.035f, 0f, 0f);
    public static Vector3 horizonKongOffset3 = new Vector3(0f, 0f, 0.035f);

    public static Vector3 verticalKongOffset0 = new Vector3(0, 0f, -0.035f);
    public static Vector3 verticalKongOffset1 = new Vector3(0.035f, 0f, 0);
    public static Vector3 verticalKongOffset2 = new Vector3(0f, 0f, 0.035f);
    public static Vector3 verticalKongOffset3 = new Vector3(-0.035f, 0f, 0f);

    public static Quaternion reverseKongOffset = Quaternion.Euler(0f, 180f, 0f);

    [Header("DoraTransform")]
    public static Vector3 doraPosition = new Vector3(-0.645f, 0.09f, 20f);
    public static Vector3 horizonDoraOffset = new Vector3(0.22f, 0f, 0f);
    public static Vector3 verticalDoraOffset = new Vector3(0f, 0f, -0.7f);
    public static Quaternion doraQuaternion = Quaternion.Euler(180f, 0f, -90f);

    //和了ったときに麻雀台に垂直に立てて牌を倒す演出に入る.
    //その時のオフセット.
    [Header("Draw&RonTransform")]
    public static Vector3 winOffset0 = new Vector3(0f, -0.035f, 0.245f);
    public static Vector3 winOffset1 = new Vector3(-0.245f, -0.035f, 0f);
    public static Vector3 winOffset2 = new Vector3(0f, -0.035f, -0.245f);
    public static Vector3 winOffset3 = new Vector3(0.245f, -0.035f, 0f);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstantiateDora(List<int> deadWall)
    {
        //王牌(今後のドラ表示牌)をネットワークを通じて 2*7 で生成.
        //常に同期する生成を多くの牌でしてしまうと重くなる可能性がある.
        int InstantiateDoraCount = 0;
        foreach (var tileNum in deadWall)
        {
            int InstantiateDoraCount_horizon = InstantiateDoraCount % 7;
            int InstantiateDoraCount_vertical = InstantiateDoraCount / 7;
            Vector3 InstantiateDoraPosition = doraPosition
                + InstantiateDoraCount_horizon * horizonDoraOffset
                + InstantiateDoraCount_vertical * verticalDoraOffset;
            PhotonView photonView = GetComponent<PhotonView>();
            photonView.RPC(nameof(MakeDora), RpcTarget.All, InstantiateDoraCount, 
                tileNum, viewId, InstantiateDoraPosition, doraQuaternion);
            viewId++;
            InstantiateDoraCount++;
        }
        instantiatedDora = true;
    }

    public void InstantiateHand(int order, List<int> hand)
    {
        int InstantiateHandCount = 0;
        foreach (var tileNum in hand)
        {
            InstantiateHandPosition = Vector3.zero;
            handQuaternion = Quaternion.identity;
            switch (order)
            {
                case 0:
                    InstantiateHandPosition = handPosition0 + InstantiateHandCount * handOffset0;
                    handQuaternion = handQuaternion0;
                    break;
                case 1:
                    InstantiateHandPosition = handPosition1 + InstantiateHandCount * handOffset1;
                    handQuaternion = handQuaternion1;
                    break;
                case 2:
                    InstantiateHandPosition = handPosition2 + InstantiateHandCount * handOffset2;
                    handQuaternion = handQuaternion2;
                    break;
                case 3:
                    InstantiateHandPosition = handPosition3 + InstantiateHandCount * handOffset3;
                    handQuaternion = handQuaternion3;
                    break;
                default:
                    break;
            }

            //常に同期する生成を多くの牌でしてしまうと重くなる可能性がある.
            //また, 自分の手牌を見やすく傾けるが, それも同期されてしまう.
            //そのため横の人から手牌が見えてしまう.
            //よって, 同期と非同期を指定できるようにPhotonのメソッドを使用せず, 自分で生成.

            PhotonView photonView = GetComponent<PhotonView>();
            photonView.RPC(nameof(MakeHand), RpcTarget.All, order, tileNum, viewId, InstantiateHandPosition, handQuaternion);
            viewId++;

            InstantiateHandCount++;
        }
        instantiatedHand = true;
    }

    public IEnumerator Draw(int order, int tileNum)
    {
        InstantiateHandPosition = Vector3.zero;
        handQuaternion = Quaternion.identity;
        GameObject container = null;
        HandContainer handContainer = null;
        switch (order)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                handContainer = container.GetComponent<HandContainer>();
                InstantiateHandPosition = handPosition0 + (handContainer.Count + 0.5f) * handOffset0;
                handQuaternion = handQuaternion0;
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                handContainer = container.GetComponent<HandContainer>();
                InstantiateHandPosition = handPosition1 + (handContainer.Count + 0.5f) * handOffset1;
                handQuaternion = handQuaternion1;
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                handContainer = container.GetComponent<HandContainer>();
                InstantiateHandPosition = handPosition2 + (handContainer.Count + 0.5f) * handOffset2;
                handQuaternion = handQuaternion2;
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                handContainer = container.GetComponent<HandContainer>();
                InstantiateHandPosition = handPosition3 + (handContainer.Count + 0.5f) * handOffset3;
                handQuaternion = handQuaternion3;
                break;
            default:
                break;
        }

        /*
         * 牌を引いたときにソートされる.
         * この時に持ち上げた牌の高さをもとに戻しておかないと,
         * ソートされたときに高さがずれた状態が初期位置となり, 
         * 再度持ち上げられるようになってしまう.
         * 引いてからしばらく持ち上げられないようにクールタイムを導入した.
        */
        bool startedCoolTime = false;
        for(int i = 0; i < handContainer.Count; i++)
        {
            StartCoroutine(handContainer[i].LiftHandCoolTime());
            handContainer[i].transform.position = handContainer[i].initialHandPosition;
            handContainer[i].myTurnEnd = false;
            if (i == handContainer.Count - 1)
            {
                startedCoolTime = true;
            }
        }
        yield return new WaitUntil(() => startedCoolTime);
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(MakeHand), RpcTarget.All, order, tileNum, viewId, InstantiateHandPosition, handQuaternion);
        soundManager.soundType = SoundManager.SOUND_TYPE.TILE;
        soundManager.PlaySoundEffect(0);
        photonView.RPC(nameof(AddHand), RpcTarget.All, order, tileNum);
        viewId++;
        easySortedHand = false;
        photonView.RPC(nameof(EasySortHand), RpcTarget.All, order);
        yield return new WaitUntil(() => easySortedHand);
        handContainer[handContainer.Count - 1].myTurnEnd = false;

        instantiatedHand = true;
    }

    //同じ牌を4枚持っているときに行える暗槓.
    //見栄えが良くなるように, 赤ドラがあれば右から2番目に来るようにしている.
    //また新しく槓するときに最後に槓した牌の向きによって牌の中心の座標を
    //調整する必要があるので適宜計算する(前回の牌の中心座標を基準に牌を置くため).
    public IEnumerator ConcealedKong(int order, int tileNum)
    {
        kongPosition = Vector3.zero;
        kongQuaternion = Quaternion.identity;
        GameObject container = null;
        GameObject kongContainer = null;
        HandContainer handContainer = null;
        HandContainer handContainer_Kong = null;
        switch (order)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong0");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if(handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                }
                else
                {
                    //最後に槓した牌の向きと高さを基準に調整している.
                    HandContainerChild handContainerChild 
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if(handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position 
                            + basicKongOffset0;
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position 
                            + basicKongOffset0 + horizonKongOffset0 - verticalKongOffset0;
                    }
                }
                kongOffset = basicKongOffset0;
                kongQuaternion = insideWallQuaternion0;
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong1");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                }
                else
                {
                    //最後に槓した牌の向きと高さを基準に調整している.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset1;
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset1 + horizonKongOffset1 - verticalKongOffset1;
                    }
                }
                kongOffset = basicKongOffset1;
                kongQuaternion = insideWallQuaternion1;
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong2");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                }
                else
                {
                    //最後に槓した牌の向きと高さを基準に調整している.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset2;
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset2 + horizonKongOffset2 - verticalKongOffset2;
                    }
                }
                kongOffset = basicKongOffset2;
                kongQuaternion = insideWallQuaternion2;
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong3");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                }
                else
                {
                    //最後に槓した牌の向きと高さを基準に調整している.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset3;
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset3 + horizonKongOffset3 - verticalKongOffset3;
                    }
                }
                kongOffset = basicKongOffset3;
                kongQuaternion = insideWallQuaternion3;
                break;
            default:
                break;
        }

        /*
        * 牌を引いたとき同様槓したときも持ち上げた牌の高さをもとに戻しておき,
        * しばらく持ち上げられないようクールタイムを導入した.
        */
        bool startedCoolTime = false;
        for (int i = 0; i < handContainer.Count; i++)
        {
            StartCoroutine(handContainer[i].LiftHandCoolTime());
            handContainer[i].transform.position = handContainer[i].initialHandPosition;
            if (i == handContainer.Count - 1)
            {
                startedCoolTime = true;
            }
        }
        yield return new WaitUntil(() => startedCoolTime);
        PhotonView photonView = GetComponent<PhotonView>();
        copyKongCount = 0;
        endKong = false;
        wetherConcealedKong = true;
        Checker checker = GetComponent<Checker>();
        //見栄えのため赤ドラがあれば右から2番目に来るように配置する.
        //また槓した牌のうち端の牌は裏向きにする.
        for (int i = 0; i < kongCount; i++)
        {
            yield return new WaitUntil(() => (i == copyKongCount));
            if (i == 1 && checker.blackTileNumsHave5.Contains(tileNum))
            {
                int redDoraTileNum = tileNum - 5;
                photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order, 
                    redDoraTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
            }
            else if(i == 0 || i == 3)
            {
                Quaternion reverseKongQuaternion = kongQuaternion * reverseKongOffset;
                photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order, 
                    tileNum, i, kongPosition, reverseKongQuaternion, wetherConcealedKong);
            }
            else
            {
                photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order, 
                    tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
            }
            kongPosition += kongOffset;
            yield return new WaitUntil(() => (i == copyKongCount - 1));
            if (i == kongCount - 1)
            {
                endKong = true;
            }
        }
        yield return new WaitUntil(() => endKong);
        soundManager.soundType = SoundManager.SOUND_TYPE.TILE;
        soundManager.PlaySoundEffect(1);
        yield return new WaitForSeconds(0.75f);
        sortedHand = false;
        photonView.RPC(nameof(SortHand), RpcTarget.All, order);
        yield return new WaitUntil(() => sortedHand);
        soundManager.PlaySoundEffect(1);
    }

    //3枚持っている牌がほかのプレイヤーに捨てられたときに行える大明槓.
    //見栄えが良くなるように, 赤ドラがあれば右から2番目に来るようにしている.
    //また新しく槓するときに最後に槓した牌の向きによって牌の中心の座標を
    //調整する必要があるので適宜計算する.
    public IEnumerator MeldedKong(int order, int nowTurn, int tileNum)
    {
        kongPosition = Vector3.zero;
        kongQuaternion = Quaternion.identity;
        GameObject container = null;
        GameObject kongContainer = null;
        HandContainer handContainer = null;
        HandContainer handContainer_Kong = null;
        int differenceInOrder = (nowTurn - order + 4) % 4;
        //槓した人と自分の順番の差.
        //つまりどこから槓したかを表す.
        //右側なら1, 対面なら2, 左側なら3.
        switch (order)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong0");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                    if(differenceInOrder == 1)
                    {
                        kongPosition += verticalKongOffset0;
                    }
                }
                else
                {
                    //最後に槓した牌の向きと高さを基準に調整している.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset0;
                        if(differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset0 + verticalKongOffset0;
                        }
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset0 + horizonKongOffset0 - verticalKongOffset0;
                        if(differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset0 + verticalKongOffset0;
                        }
                    }
                }
                kongOffset = basicKongOffset0;
                horizonKongOffset = horizonKongOffset0;
                verticalKongOffset = verticalKongOffset0;
                kongQuaternion = insideWallQuaternion0;
                anotherKongQuaternion = insideWallQuaternion1;
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong1");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                    if (differenceInOrder == 1)
                    {
                        kongPosition += verticalKongOffset1;
                    }
                }
                else
                {
                    //最後に槓した牌の向きと高さを基準に調整している.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset1;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset1 + verticalKongOffset1;
                        }
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset1 + horizonKongOffset1 - verticalKongOffset1;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset1 + verticalKongOffset1;
                        }
                    }
                }
                kongOffset = basicKongOffset1;
                horizonKongOffset = horizonKongOffset1;
                verticalKongOffset = verticalKongOffset1;
                kongQuaternion = insideWallQuaternion1;
                anotherKongQuaternion = insideWallQuaternion2;
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong2");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                    if (differenceInOrder == 1)
                    {
                        kongPosition += verticalKongOffset2;
                    }
                }
                else
                {
                    //最後に槓した牌の向きと高さを基準に調整している.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset2;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset2 + verticalKongOffset2;
                        }
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset2 + horizonKongOffset2 - verticalKongOffset2;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset2 + verticalKongOffset2;
                        }
                    }
                }
                kongOffset = basicKongOffset2;
                horizonKongOffset = horizonKongOffset2;
                verticalKongOffset = verticalKongOffset2;
                kongQuaternion = insideWallQuaternion2;
                anotherKongQuaternion = insideWallQuaternion3;
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong3");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                    if (differenceInOrder == 1)
                    {
                        kongPosition += verticalKongOffset3;
                    }
                }
                else
                {
                    //最後に槓した牌の向きと高さを基準に調整している.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset3;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset3 + verticalKongOffset3;
                        }
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset3 + horizonKongOffset3 - verticalKongOffset3;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset3 + verticalKongOffset3;
                        }
                    }
                }
                kongOffset = basicKongOffset3;
                horizonKongOffset = horizonKongOffset3;
                verticalKongOffset = verticalKongOffset3;
                kongQuaternion = insideWallQuaternion3;
                anotherKongQuaternion = insideWallQuaternion0;
                break;
            default:
                break;
        }

        /*
        * 牌を引いたとき同様槓したときも持ち上げた牌の高さをもとに戻しておき,
        * しばらく持ち上げられないようクールタイムを導入した.
        */
        bool startedCoolTime = false;
        for (int i = 0; i < handContainer.Count; i++)
        {
            StartCoroutine(handContainer[i].LiftHandCoolTime());
            handContainer[i].transform.position = handContainer[i].initialHandPosition;
            if (i == handContainer.Count - 1)
            {
                startedCoolTime = true;
            }
        }
        yield return new WaitUntil(() => startedCoolTime);
        PhotonView photonView = GetComponent<PhotonView>();
        copyKongCount = 0;
        endKong = false;
        wetherConcealedKong = false;
        Checker checker = GetComponent<Checker>();

        //見栄えのため赤ドラがあれば右から2番目に来るように配置する.
        //大明槓した他人の席の位置に合わせて他人の牌を横向きに,自分の牌を縦向きに置く.
        //右側の人なら4枚のうち最も右, 左側の人なら最も左,
        //対面なら左から2番目に他人の牌を置く.
        for (int i = 0; i < kongCount; i++)
        {
            yield return new WaitUntil(() => (i == copyKongCount));

            switch (i)
            {
                case 0:
                    //右側の人から槓したかどうか.
                    if(differenceInOrder == 1)
                    {
                        if (handContainer_Kong.Count == 0)
                        {
                            kongPosition -= verticalKongOffset;
                        }
                        photonView.RPC(nameof(MoveDiscardedTile), RpcTarget.All, order, nowTurn,
                            tileNum, i, kongPosition, anotherKongQuaternion);
                    }
                    else
                    {
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    break;
                case 1:
                    //右側の人から槓したかどうか.
                    if (differenceInOrder == 1)
                    {
                        kongPosition += kongOffset + horizonKongOffset - verticalKongOffset;
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else if (checker.blackTileNumsHave5.Contains(tileNum))
                        {
                            int redDoraTileNum = tileNum - 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                redDoraTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    else
                    {
                        kongPosition += kongOffset;
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else if (checker.blackTileNumsHave5.Contains(tileNum))
                        {
                            int redDoraTileNum = tileNum - 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                redDoraTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    break;
                case 2:
                    //対面の人から槓したかどうか.
                    if (differenceInOrder == 2)
                    {
                        kongPosition += kongOffset + horizonKongOffset + verticalKongOffset;
                        photonView.RPC(nameof(MoveDiscardedTile), RpcTarget.All, order, nowTurn,
                            tileNum, i, kongPosition, anotherKongQuaternion);
                    }
                    else
                    {
                        kongPosition += kongOffset;
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    break;
                case 3:
                    //左側の人から槓したかどうか.
                    if (differenceInOrder == 3)
                    {
                        kongPosition += kongOffset + horizonKongOffset + verticalKongOffset;
                        photonView.RPC(nameof(MoveDiscardedTile), RpcTarget.All, order, nowTurn,
                            tileNum, i, kongPosition, anotherKongQuaternion);
                    }
                    //右側の人から槓したかどうか.
                    else if (differenceInOrder == 1)
                    {
                        kongPosition += kongOffset;
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    //対面の人から槓したかどうか.
                    else if (differenceInOrder == 2)
                    {
                        kongPosition += kongOffset + horizonKongOffset - verticalKongOffset;
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    break;
                default:
                    break;
            }

            yield return new WaitUntil(() => (i == copyKongCount - 1));
            if (i == kongCount - 1)
            {
                endKong = true;
            }
        }
        yield return new WaitUntil(() => endKong);
        soundManager.soundType = SoundManager.SOUND_TYPE.TILE;
        soundManager.PlaySoundEffect(1);
        yield return new WaitForSeconds(0.75f);
        sortedHand = false;
        photonView.RPC(nameof(SortHand), RpcTarget.All, order);
        yield return new WaitUntil(() => sortedHand);
        soundManager.PlaySoundEffect(1);
    }

    //和了った時に牌を麻雀台に垂直に立て, 倒す演出.
    public IEnumerator WinMove(int myOrder, int winnerOrder)
    {
        GameObject container = null;
        Vector3 tiltPosition = Vector3.zero;
        Quaternion handQuaternion = Quaternion.identity; ;
        Quaternion tiltQuaternion = Quaternion.identity; ;
        switch (winnerOrder)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                tiltPosition = winOffset0;
                handQuaternion = handQuaternion0;
                tiltQuaternion = insideWallQuaternion0;
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                tiltPosition = winOffset1;
                handQuaternion = handQuaternion1;
                tiltQuaternion = insideWallQuaternion1;
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                tiltPosition = winOffset2;
                handQuaternion = handQuaternion2;
                tiltQuaternion = insideWallQuaternion2;
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                tiltPosition = winOffset3;
                handQuaternion = handQuaternion3;
                tiltQuaternion = insideWallQuaternion3;
                break;
            default:
                break;
        }

        HandContainer handContainer = container.GetComponent<HandContainer>();
        if (myOrder == winnerOrder)
        {
            foreach (HandContainerChild handContainerChild in handContainer)
            {
                handContainerChild.order = -1;
                handContainerChild.transform.position = handContainerChild.initialHandPosition;
                handContainerChild.transform.rotation = handQuaternion;
            }
            easySortedHand = false;
            photonView.RPC(nameof(EasySortHand), RpcTarget.All, winnerOrder);
            yield return new WaitUntil(() => easySortedHand);
        }
        yield return new WaitForSeconds(0.5f);
        tiltedAllHands = false;
        photonView.RPC(nameof(TiltAllHands), RpcTarget.All, winnerOrder, 
            tiltPosition, tiltQuaternion);
        yield return new WaitUntil(() => tiltedAllHands);
        soundManager.soundType = SoundManager.SOUND_TYPE.TILE;
        soundManager.PlaySoundEffect(2);

        easySortedHand = false;
        photonView.RPC(nameof(EasySortHand), RpcTarget.All, winnerOrder);
        yield return new WaitUntil(() => easySortedHand);
    }

    //[PunRPC]は関数を同期するためのもの.
    [PunRPC]
    private void MakeDora(int doraIndex, int tileNum, int viewId, Vector3 InstantiateDoraPosition, Quaternion doraQuaternion)
    {
        handContainerChild = tiles[tileNum].GetComponent<HandContainerChild>();
        handContainerChild.order = -1;
        handContainerChild.doraIndex = doraIndex;
        handContainerChild.tileNum = tileNum;

        //同期を外す処理もできるように, 
        //ネットワークオブジェクトを独自に生成し, ネットワークIDを独自に設定する.
        GameObject doraTile = Instantiate(tiles[tileNum],
            InstantiateDoraPosition, doraQuaternion);
        PhotonView photonView = doraTile.GetComponent<PhotonView>();
        photonView.ViewID = viewId;

        //一括管理できるように親オブジェクトを指定する.
        GameObject container = GameObject.FindWithTag("DoraContainer");
        if (container != null)
        {
            doraTile.transform.SetParent(container.transform);
        }
    }

    [PunRPC]
    public void MakeHand(int order, int tileNum, int viewId, Vector3 InstantiateHandPosition, Quaternion handQuaternion)
    {
        handContainerChild = tiles[tileNum].GetComponent<HandContainerChild>();
        handContainerChild.order = order;
        handContainerChild.tileNum = tileNum;
        handContainerChild.verticalDirection = 1;
        handContainerChild.tileName = tiles[tileNum].name;
        handContainerChild.myTurnEnd = true;
        handContainerChild.isWaited = false;
        handContainerChild.initialHandPosition = InstantiateHandPosition;
        handContainerChild.canLift = true;
        handContainerChild.canDiscard = false;

        //同期を外す処理もできるように, 
        //ネットワークオブジェクトを独自に生成し, ネットワークIDを独自に設定する.
        GameObject handTile = Instantiate(tiles[tileNum],
            InstantiateHandPosition, handQuaternion);
        PhotonView photonView = handTile.GetComponent<PhotonView>();
        photonView.ViewID = viewId;

        //一括管理できるように親オブジェクトを指定する.
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
        if (container != null)
        {
            handTile.transform.SetParent(container.transform);
        }
    }

    [PunRPC]
    public void AddHand(int order, int tileNum)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.people[order].hand.Add(tileNum);
    }

    //通常のソート.
    //牌を引いてきたときや牌を捨てたとき, 槓したときにソートを行う.
    //牌が順番に並び替えられるように位置を変更する.
    [PunRPC]
    public void SortHand(int order)
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


        //牌の名前でソートする.
        //というのも萬子の5の赤ドラと萬子の5はそれぞれ数字では 0, 5 と区別しているから.
        //萬子：A 数字 (赤ドラならRが付く), 筒子：B 数字 (略), 索子：C 数字 (略), 字牌：H 数字.
        //番号でソートすると2つが同じ牌と認識されず離れて手牌に置かれてしまう.

        //(以下補足：かといって名前で判断するとプログラム的に処理しにくく, できるだけ番号で扱いたかった.
        //結果番号と名前両方割り当てた).
        HandContainer handContainer = container.GetComponent<HandContainer>();
        List<HandContainerChild> handGOList = handContainer.handGOList;
        handGOList = handGOList.OrderBy(child => child.name).ToList();
        handContainer.handGOList = handGOList;
        Vector3 NewLocalPosition = Vector3.zero;

        for (int i = 0; i < handGOList.Count; i++)
        {
            handContainerChild = handGOList[i].GetComponent<HandContainerChild>();
            handContainerChild.handIndex = i;
            StartCoroutine(handContainerChild.LiftHandCoolTime());
            Transform child = handGOList[i].transform;
            switch (order)
            {
                case 0:
                    NewLocalPosition = i * handOffset0;
                    break;
                case 1:
                    NewLocalPosition = i * handOffset1;
                    break;
                case 2:
                    NewLocalPosition = i * handOffset2;
                    break;
                case 3:
                    NewLocalPosition = i * handOffset3;
                    break;
                default:
                    break;
            }
            child.localPosition = NewLocalPosition;
            handContainerChild.initialHandPosition = child.position;
        }
        sortedHand = true;
    }

    //こちらのソートはソートというよりはソートされた牌の内部番号が, 
    //牌を引いてきたときにずれないようにするためのもの.
    //手牌を引いてきたときや捨てたときに,
    //自動で親オブジェクトに内部番号が割り当てられるようにしている.
    //その際に引いた順に更新されてしまう.
    //それをソートされていたままの順に戻すためのソート.
    //内部番号を変更するだけで牌同士の位置は変更しない.
    [PunRPC]
    public void EasySortHand(int order)
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

        HandContainer handContainer = container.GetComponent<HandContainer>();
        List<HandContainerChild> handGOList = handContainer.handGOList;
        HandContainerChild lastHandContainerChild = handGOList[handContainer.Count - 1];
        lastHandContainerChild.handIndex = handContainer.Count - 1;
        handGOList.RemoveAt(handContainer.Count - 1);
        handGOList = handGOList.OrderBy(child => child.name).ToList();
        handGOList.Add(lastHandContainerChild);
        handContainer.handGOList = handGOList;
        StartCoroutine(lastHandContainerChild.LiftHandCoolTime());

        easySortedHand = true;
    }

    //手牌から槓した人の槓した牌を置く場所に移動させるメソッド.
    //この時一括管理できるように親オブジェクトの変更や,
    //手牌と認識して持ち上げられないように所有者なしにする.
    [PunRPC]
    private void MoveToKongTile(int order, int tileNum, int kongCount,
    Vector3 kongPosition, Quaternion kongQuaternion, bool wetherConcealedKong)
    {
        GameObject container = null;
        GameObject kongContainer = null;
        HandContainer handContainer = null;
        HandContainer handContainer_Kong = null;
        switch (order)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                kongContainer = GameObject.FindWithTag("Kong0");
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                kongContainer = GameObject.FindWithTag("Kong1");
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                kongContainer = GameObject.FindWithTag("Kong2");
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                kongContainer = GameObject.FindWithTag("Kong3");
                break;
            default:
                break;
        }
        handContainer = container.GetComponent<HandContainer>();
        handContainer_Kong = kongContainer.GetComponent<HandContainer>();
        foreach (HandContainerChild handContainerChild in handContainer)
        {
            if (handContainerChild.tileNum == tileNum)
            {
                handContainerChild.transform.parent = null;
                handContainerChild.order = -1;  //牌を手牌として認識し, 持ち上げられないように所有者なしにする.
                handContainerChild.transform.rotation = kongQuaternion;
                handContainerChild.transform.position = kongPosition;
                if (kongContainer != null)
                {
                    handContainerChild.transform.SetParent(kongContainer.transform);
                }
                break;
            }
        }
        GameObject gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        List<int> ownerHand = gameManager.people[order].hand;
        ownerHand.Remove(tileNum);
        List<int> ownerKongTiles = gameManager.people[order].kongTiles;
        ownerKongTiles.Add(tileNum);

        //4枚で槓1回できるので最後の牌で槓した回数をインクリメント.
        if (kongCount == 3)
        {
            gameManager.people[order].kongCount++;
            if (wetherConcealedKong)
            {
                gameManager.people[order].concealedKongCount++;
            }
            else
            {
                gameManager.people[order].meldedKongCount++;
            }
        }
        TileController tileController = gameController.GetComponent<TileController>();
        tileController.copyKongCount++;
    }

    //河から槓した人の槓した牌を置く場所に移動させるメソッド.
    //この時一括管理できるように親オブジェクトの変更や,
    //手牌と認識して持ち上げられないように所有者なしにする.
    [PunRPC]
    private void MoveDiscardedTile(int order, int nowTurn, int tileNum, int kongCount,
        Vector3 kongPosition, Quaternion kongQuaternion)
    {
        GameObject insideWallContainer = null;
        GameObject kongContainer = null;
        HandContainer handContainer_InsideWall = null;
        HandContainer handContainer_Kong = null;
        switch (order)
        {
            case 0:
                kongContainer = GameObject.FindWithTag("Kong0");
                break;
            case 1:
                kongContainer = GameObject.FindWithTag("Kong1");
                break;
            case 2:
                kongContainer = GameObject.FindWithTag("Kong2");
                break;
            case 3:
                kongContainer = GameObject.FindWithTag("Kong3");
                break;
            default:
                break;
        }
        handContainer_Kong = kongContainer.GetComponent<HandContainer>();
        switch (nowTurn)
        {
            case 0:
                insideWallContainer = GameObject.FindWithTag("InsideWall0");
                break;
            case 1:
                insideWallContainer = GameObject.FindWithTag("InsideWall1");
                break;
            case 2:
                insideWallContainer = GameObject.FindWithTag("InsideWall2");
                break;
            case 3:
                insideWallContainer = GameObject.FindWithTag("InsideWall3");
                break;
            default:
                break;
        }
        handContainer_InsideWall = insideWallContainer.GetComponent<HandContainer>();
        HandContainerChild handContainerChild 
            = handContainer_InsideWall[handContainer_InsideWall.Count - 1];
        handContainerChild.transform.parent = null;
        handContainerChild.order = -1;  //牌を手牌として認識し, 持ち上げられないように所有者なしにする.
        handContainerChild.transform.rotation = kongQuaternion;
        handContainerChild.transform.position = kongPosition;
        if (kongContainer != null)
        {
            handContainerChild.transform.SetParent(kongContainer.transform);
        }

        GameObject gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        List<int> ownerHand = gameManager.people[order].hand;
        ownerHand.Remove(tileNum);
        List<int> ownerKongTiles = gameManager.people[order].kongTiles;
        ownerKongTiles.Add(tileNum);

        //4枚で槓1回できるので最後の牌で槓した回数をインクリメント.
        if (kongCount == 3)
        {
            gameManager.people[order].kongCount++;
            gameManager.people[order].meldedKongCount++;
        }
        TileController tileController = gameController.GetComponent<TileController>();
        tileController.copyKongCount++;
    }

    //和了った時に牌を倒すメソッド.
    [PunRPC]
    private void TiltAllHands(int winnerOrder, Vector3 tiltPosition, Quaternion tiltQuaternion)
    {
        GameObject container = null;
        switch (winnerOrder)
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
        HandContainer handContainer = container.GetComponent<HandContainer>();
        foreach (HandContainerChild handContainerChild in handContainer)
        {
            handContainerChild.transform.localPosition += tiltPosition;
            handContainerChild.transform.rotation = tiltQuaternion;
        }
        tiltedAllHands = true;
    }
}
