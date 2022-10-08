using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class FireScript : MonoBehaviour
{
    public PhotonView Pv;
    int dir;

    void Start() => Destroy(gameObject, 2f);

    void Update() => transform.Translate(Vector3.right * 7 * Time.deltaTime * dir);

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag == "Ground")
            Pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        if(!Pv.IsMine && col.tag == "Player" && col.GetComponent<PhotonView>().IsMine)
        {
            col.GetComponent<DragonScript>().Hit();
            Pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DirRPC(int dir) => this.dir = dir;

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
