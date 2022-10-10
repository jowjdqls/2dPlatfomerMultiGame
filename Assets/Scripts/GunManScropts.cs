using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using Cinemachine;

public class GunManScropts : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D Rb;
    public Animator An;
    public SpriteRenderer Sr;
    public PhotonView Pv;
    public Text NickNameText;
    public Image HealthImage;

    bool isGround;
    Vector3 curPos;

    private void Awake() 
    {
        //이름표시
        NickNameText.text = Pv.IsMine ? PhotonNetwork.NickName : Pv.Owner.NickName;
        NickNameText.color = Pv.IsMine ? Color.green : Color.red;

        if(Pv.IsMine)
        {
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;
        }
    }

    private void FixedUpdate() 
    {
        if(Pv.IsMine)
        {
            float axis = Input.GetAxisRaw("Horizontal");
            Rb.velocity = new Vector2(4 * axis, Rb.velocity.y);

            if(axis != 0)
            {
                An.SetBool("GunMan_walk", true);
                Pv.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
            }
            else An.SetBool("GunMan_walk", false);

            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1<<LayerMask.NameToLayer("Ground"));
            An.SetBool("GunMan_jump", !isGround);
            if(Input.GetKeyDown(KeyCode.Space) && isGround) Pv.RPC("JumpRPC", RpcTarget.All);

            if(Input.GetKeyDown(KeyCode.Z))
            {
                PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(Sr.flipX ? -0.4f : 0.4f, -0.11f, 0), Quaternion.identity)
                .GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, Sr.flipX ? -1 : 1);
                An.SetTrigger("GunMan_shot");
            }
        } 
        else if((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    [PunRPC]
    void FlipXRPC(float axis) => Sr.flipX = axis == -1;

    [PunRPC]
    void JumpRPC()
    {
        Rb.velocity = Vector2.zero;
        Rb.AddForce(Vector2.up * 700);
    }

    public void Hit()
    {
        HealthImage.fillAmount -= 0.1f;
        if(HealthImage.fillAmount <= 0)
        {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            Pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(HealthImage.fillAmount);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }

    void OnTriggerExit2D(Collider2D other) 
    {
        if(other.tag == "CMRange")
        {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            Pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }    
    }
}
