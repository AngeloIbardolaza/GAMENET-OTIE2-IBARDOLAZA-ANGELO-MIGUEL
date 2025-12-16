using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetPlayerCam : NetworkBehaviour
{
    public Camera playerCam;

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        this.playerCam.gameObject.SetActive(true);
    }
}
