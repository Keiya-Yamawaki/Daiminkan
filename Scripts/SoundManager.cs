using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SoundManager : MonoBehaviourPunCallbacks
{
    private AudioSource audioSource;
    public AudioClip[] characterSE;
    public AudioClip[] tileSE;
    public AudioClip[] buttonSE;
    public AudioClip[] winnerSE;
    public AudioClip[] endVoice;

    public enum SOUND_TYPE
    {
        NONE = -1,
        CHARACTER = 0,
        TILE,
        MYTILE,
        BUTTON,
        WINNER,
        END_VOICE,
        NUM,
    };

    public SOUND_TYPE soundType = SOUND_TYPE.NONE;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySoundEffect(int soundIndex)
    {
        if(soundType == SOUND_TYPE.CHARACTER || soundType == SOUND_TYPE.TILE)
        {
            //PlayTileSoundEffectRPC 関数をルーム内メンバー全員で同期し, 実行している.
            photonView.RPC("PlayTileSoundEffectRPC", RpcTarget.All, soundType, soundIndex);
        }
        else
        {
            switch (soundType)
            {
                case SOUND_TYPE.MYTILE:
                    audioSource.PlayOneShot(tileSE[soundIndex]);
                    break;
                case SOUND_TYPE.BUTTON:
                    audioSource.PlayOneShot(buttonSE[soundIndex]);
                    break;
                case SOUND_TYPE.WINNER:
                    audioSource.PlayOneShot(winnerSE[soundIndex]);
                    break;
                case SOUND_TYPE.END_VOICE:
                    audioSource.PlayOneShot(endVoice[soundIndex]);
                    break;
                default:
                    break;
            }
        }

    }

    //[PunRPC]は関数をPhotonネットワーク上で同期するためのもの.
    [PunRPC]
    private void PlayTileSoundEffectRPC(SOUND_TYPE soundType, int soundIndex)
    {
        switch (soundType)
        {
            case SOUND_TYPE.CHARACTER:
                audioSource.PlayOneShot(characterSE[soundIndex]);
                break;
            case SOUND_TYPE.TILE:
                audioSource.PlayOneShot(tileSE[soundIndex]);
                break;
            default:
                break;
        }
    }
}
