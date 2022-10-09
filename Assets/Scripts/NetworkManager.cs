using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("StartPanel")]
    public InputField NickNameInput;

    [Header("LobbyPanel")]
    public GameObject LobbyPanel;
    public InputField RoomInput;
    public Text WelcomeText;
    public Text LobbyInfoText;
    public Button[] CellBtu;
    public Button PreviousBtu;
    public Button NextBtu;
    
    [Header("aaaaa")]
    public GameObject StartPanel;
    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;
    public Image GunManImgae;
    public Image DragonImgae;
    public bool GunBtu;
    public bool DragonBtu;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

    //방리스트 갱신
    public void MyListClick(int num)
    {
        if(num == -2)  --currentPage;
        else if(num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        MyListRenewal();
    }

    void MyListRenewal()
    {
        maxPage = (myList.Count % CellBtu.Length == 0) ? myList.Count / CellBtu.Length : myList.Count / CellBtu.Length + 1;

        PreviousBtu.interactable = (currentPage <= 1) ? false : true;
        NextBtu.interactable = (currentPage >= maxPage) ? false : true;

        multiple = (currentPage - 1) * CellBtu.Length;
        for(int i = 0; i < CellBtu.Length; i++)
        {
            CellBtu[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtu[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtu[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for(int i = 0; i < roomCount; i++)
        {
            if(!roomList[i].RemovedFromList)
            {
                if(!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }

    private void Awake() 
    {
        Screen.SetResolution(1920, 1080, false);
        PhotonNetwork.SendRate= 60;
        PhotonNetwork.SerializationRate = 30;    
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();
    /*{ 로비 전
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions {MaxPlayers = 2}, null);
    }*/

    public override void OnJoinedLobby()
    {
        LobbyPanel.SetActive(true);
        StartPanel.SetActive(false);
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다";
    }

    //로비 전
    /*public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(false);
        StartCoroutine("DestroyBullet");
        Spawn();
    }*/

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach(GameObject GO in GameObject.FindGameObjectsWithTag("Bullet")) GO.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
    }

    void Update() 
    {
        LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속";
        if(Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
    }

    public void Spawn()
    {   
        DisconnectPanel.SetActive(false);
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
        GunManImgae.color = new Color(1,1,1,0.4f);
        DragonImgae.color = new Color(255, 255, 255, 255);
    }

    public void DragonCh()
    {
        GunBtu = false;
        DragonBtu = true;
        DragonImgae.color = new Color(1,1,1,0.4f);
        GunManImgae.color = new Color(255, 255, 255, 255);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        StartPanel.SetActive(true);
        LobbyPanel.SetActive(false);
        DisconnectPanel.SetActive(false);
    }
    
    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions {MaxPlayers = 2});

    public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(true);
        LobbyPanel.SetActive(false);
        Invoke("Spawn", 3f);
    }
}
