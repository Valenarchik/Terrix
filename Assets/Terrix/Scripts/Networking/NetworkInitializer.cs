using FishNet.Object;
using Terrix.Controllers;
using Terrix.Controllers.Country;
using Terrix.DTO;
using Terrix.Game.GameRules;
using Terrix.Networking;
using UnityEngine;

namespace Terrix
{
    public class NetworkInitializer : NetworkBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [SerializeField] private Lobby lobby;
        [SerializeField] private MainMapEntryPoint mainMapEntryPoint;
        [SerializeField] private MainMapCameraController mainMapCameraController;
        [SerializeField] private CountryController countryController;

        // public override void OnStartServer()
        // {
        //     // countryController.Init_OnServer();
        //     // mainMapCameraController.Init_OnServer();
        // }
        //
        // public override void OnStartClient()
        // {
        //     // countryController.Init_OnClient();
        //     // mainMapCameraController.Init_OnClient();
        // }
    }
}