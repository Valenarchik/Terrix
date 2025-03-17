using Terrix.Game.GameRules;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Terrix.Controllers
{
    public class MainMapCameraController : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Tilemap mapTilemap;
        [SerializeField] private new Camera camera;

        [Header("Settings")] 
        [SerializeField] private float cameraSpeed = 10;
        [SerializeField] private float zoomValueOnCameraSpeedModifier = 0.25f;
        [SerializeField] private float zoomSpeed = 1000;
        [SerializeField] private float zoomValueOnZoomSpeedModifier = 0.25f;

        [Header("Boundaries")] 
        [SerializeField] private float minZoomValue = 5;
        [SerializeField] private float maxZoomValue = 100;

        private Vector2 cameraMoveDirection;
        
        private float zoomDirection;

        private Vector3 origin;
        private Vector3 difference;
        private bool drag;

        public bool Enable { get; set; }
        public bool EnableDrag { get; set; } = true;
        public bool EnableZoom { get; set; } = true;
        public bool EnableMove { get; set; } = true;

        private float ZoomValue
        {
            get => camera.orthographicSize;
            set => camera.orthographicSize = value;
        }

        public void OnMoveCamera(InputAction.CallbackContext context)
        {
            cameraMoveDirection = context.ReadValue<Vector2>();
        }

        public void OnZoomCamera(InputAction.CallbackContext context)
        {
            zoomDirection = context.ReadValue<float>();
        }
        
        public void OnDragCamera(InputAction.CallbackContext context)
        {
            var mousePosition = context.ReadValue<Vector2>();

            difference = camera.ScreenToWorldPoint(mousePosition) - camera.transform.position;
            if (!drag)
            {
                origin = camera.ScreenToWorldPoint(mousePosition);
            }

            drag = !context.canceled;
        }

        private void Start()
        {
            MainMap.Events.OnGameReady(OnGameReady);
        }

        private void OnGameReady()
        {
            Enable = true;
        }

        private void Update()
        {
            if (!Enable)
            {
                return;
            }

            if (drag)
            {
                DragCamera();
            }
            else
            {
                MoveCamera();
            }
            ZoomCamera();
        }

        private void DragCamera()
        {
            if (!EnableDrag)
            {
                return;
            }
            
            var cameraTransform = camera.transform;

            var newPosition = origin - difference;

            newPosition = AssignCameraPosition(newPosition);
            
            cameraTransform.position = newPosition;
        }

        private void MoveCamera()
        {
            if (!EnableMove)
            {
                return;
            }
            
            var cameraTransform = camera.transform;

            var cameraSpeedModifier = ZoomValue * zoomValueOnCameraSpeedModifier;
            cameraSpeedModifier = cameraSpeedModifier == 0 ? 1 : cameraSpeedModifier;

            var offset = Time.deltaTime * cameraSpeed * cameraSpeedModifier * new Vector3(cameraMoveDirection.x, cameraMoveDirection.y);
            var newPosition = cameraTransform.position + offset;

            newPosition = AssignCameraPosition(newPosition);

            cameraTransform.position = newPosition;
        }
        
        private void ZoomCamera()
        {
            if (!EnableZoom)
            {
                return;
            }
            
            var zoomSpeedModifier = ZoomValue * zoomValueOnZoomSpeedModifier;
            zoomSpeedModifier = zoomSpeedModifier == 0 ? 1 : zoomSpeedModifier;

            var offset = Time.deltaTime * zoomSpeed * zoomSpeedModifier * zoomDirection;
            var newZoomValue = ZoomValue + offset;

            newZoomValue = AssignZoom(newZoomValue);

            ZoomValue = newZoomValue;
        }

        private Vector3 AssignCameraPosition(Vector3 cameraPosition)
        {
            var bounds = CalculateBounds();

            cameraPosition.x = Mathf.Clamp(cameraPosition.x, bounds.min.x, bounds.max.x);
            cameraPosition.y = Mathf.Clamp(cameraPosition.y, bounds.min.y, bounds.max.y);

            return cameraPosition;
        }

        private Bounds CalculateBounds()
        {
            var minPos = mapTilemap.GetCellCenterWorld(Vector3Int.zero);
            var maxPos = mapTilemap.GetCellCenterWorld(mapTilemap.size - new Vector3Int(1,1,1));

            var bounds = new Bounds();
            bounds.SetMinMax(minPos, maxPos);

            return bounds;
        }

        private float AssignZoom(float zoomValue)
        {
            zoomValue = Mathf.Clamp(zoomValue, minZoomValue, maxZoomValue);
            return zoomValue;
        }
    }
}