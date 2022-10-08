using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField NickNameInput;
    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;
    public bool GunBtu;
    public bool DragonBtu;

    private void Awake() 
    {
        Screen.SetResolution(1920, 1080, false);
        PhotonNetwork.SendRate= 60;
        PhotonNetwork.SerializationRate = 30;    
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions {MaxPlayers = 6}, null);
    }

    public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(false);
        StartCoroutine("DestroyBullet");
        Spawn();
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach(GameObject GO in GameObject.FindGameObjectsWithTag("Bullet")) GO.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
    }

    void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
    }

    public void Spawn()
    {   
        if(GunBtu == true)
        {
            PhotonNetwork.Instantiate("GunMan", new Vector3(Random.Range(-13f, 13f), 4f, 0), Quaternion.identity);
            RespawnPanel.SetActive(false);
        }
        else if(DragonBtu == true)
        {
            PhotonNetwork.Instantiate("Dragon", new Vector3(Random.Range(-13, 13f), 4f, 0), Quaternion.identity);
            RespawnPanel.SetActive(false);
        }
    }

    public void GunMan()
    {
        GunBtu = true;
        DragonBtu = false;
    }

    public void DragonCh()
    {
        GunBtu = false;
        DragonBtu = true;
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        RespawnPanel.SetActive(false);
    }
}
