using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

namespace Terrix
{
    public class ServerObjectsDestroyer : NetworkBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [SerializeField] private List<GameObject> redundantObjects = new ();

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public override void OnStartServer()
        {
            foreach (var redundantObject in redundantObjects)
            {
                Destroy(redundantObject);
            }
        }
    }
}