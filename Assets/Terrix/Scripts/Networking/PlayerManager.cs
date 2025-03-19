using System;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public NetworkObject Player { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayer(NetworkObject player)
    {
        Player = player;
    }
}