using FishNet.Object;
using Terrix.Controllers;
using Terrix.Game.GameRules;
using Terrix.Networking;
using UnityEngine;

namespace Terrix
{
    public class NetworkInitializer : NetworkBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [SerializeField] private MainMapEntryPoint mainMapEntryPoint;
        [SerializeField] private MainMapCameraController mainMapCameraController;
        [SerializeField] private Lobby lobby;

        public override void OnStartServer()
        {
            
        }
    }
}