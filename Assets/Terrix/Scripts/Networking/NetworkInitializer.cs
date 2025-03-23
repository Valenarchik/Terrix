using FishNet.Object;
using Terrix.Controllers;
using Terrix.Controllers.Country;
using Terrix.Game.GameRules;
using Terrix.Networking;
using UnityEngine;

namespace Terrix
{
    public class NetworkInitializer : NetworkBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        // [SerializeField] private Lobby lobby;
        // [SerializeField] private MainMapEntryPoint mainMapEntryPoint;
        [SerializeField] private MainMapCameraController mainMapCameraController;
        [SerializeField] private CountryController countryController;

        public override void OnStartServer()
        {
            // lobby.Init_OnServer();
            // mainMapEntryPoint.Init_OnServer();
            countryController.Init_OnServer();
            mainMapCameraController.Init_OnServer();
        }

        public override void OnStartClient()
        {
            // lobby.Init_OnClient();
            // mainMapEntryPoint.Init_OnClient();
            countryController.Init_OnClient();
            mainMapCameraController.Init_OnClient();
        }
    }
}