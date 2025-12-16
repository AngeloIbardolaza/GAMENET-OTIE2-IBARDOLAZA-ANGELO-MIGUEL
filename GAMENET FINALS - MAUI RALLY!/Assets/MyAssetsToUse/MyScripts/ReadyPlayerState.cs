using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReadyPlayerState : MonoBehaviour
{
    MyNetworkRoomPlayer _roomPlayer;
    [SerializeField]
    TextMeshProUGUI nameText;
    [SerializeField]
    TextMeshProUGUI readyText;

    public MyNetworkRoomPlayer RoomPlayer => _roomPlayer;

    private void Start()
    {
        SetReady(false);
    }

    public void SetRoomPlayer(MyNetworkRoomPlayer roomPlayer)
    {
        _roomPlayer = roomPlayer;
        _roomPlayer.OnChangeReady += SetReady;
        _roomPlayer.OnChangeName += SetName;
        _roomPlayer.OnDisconnected += DestroyState;
    }

    public void DestroyState(bool _)
    {
        Destroy(this);
    }

    public void SetName(string playerName)
    {
        nameText.text = playerName;
    }

    public void SetReady(bool isReady)
    {
        readyText.text = isReady ? "Ready" : "Not Ready";
        readyText.color = isReady ? Color.green : Color.red;
    }
}
