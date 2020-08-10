using UnityEngine;
using System.Drawing;

namespace Strikeforce
{
    public class PlayerCamera : Entity
    {
        protected Camera mainCamera;

        protected override void Awake()
        {
            base.Awake();

            this.mainCamera = GameObject.FindGameObjectWithTag(Tags.MAIN_CAMERA).GetComponent<Camera>();
        }

        public void StartGame()
        {
            // Set viewport
            mainCamera.rect = new Rect(0.2f, 0f, 0.6f, 1f);
        }

        public void SetOverheadCameraPosition(Vector3 position)
        {
            float x = position.x;
            float z = position.z;
            SetOverheadCameraPosition(new Vector2(x, z));
        }

        public void SetOverheadCameraPosition(Vector2 position)
        {
            float x = position.x;
            float y = position.y;
            KeepLevelInMainView(ref x, ref y);

            Vector3 overheadView = new Vector3(x, 10, y);

            mainCamera.transform.position = overheadView;
            mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        }

        public void MoveMainCamera(float deltaX, float deltaY, float deltaZ)
        {
            Vector3 currentPosition = transform.position;

            KeepLevelInMainView(currentPosition.x, currentPosition.z, ref deltaX, ref deltaZ);

            mainCamera.transform.Translate(deltaX, deltaZ, deltaY);
        }

        public void SetMainCameraPosition(float x, float z)
        {
            float y = mainCamera.transform.position.y;
            SetMainCameraPosition(new Vector3(x, y, z));
        }

        public void SetMainCameraPosition(Vector3 position)
        {
            mainCamera.transform.position = new Vector3(position.x, position.y, position.z);
        }

        public void SetMainCameraVelocity(float velocityX, float velocityY, float velocityZ)
        {
            Rigidbody cameraVelocity = mainCamera.GetComponent<Rigidbody>();
            if (cameraVelocity == null)
            {
                Debug.Log("Main camera has no rigidbody");
                return;
            }

            cameraVelocity.velocity = new Vector3(velocityX, velocityY, velocityZ);
        }

        public RectangleF GetMainCameraViewBounds()
        {
            float x = mainCamera.transform.position.x;
            float y = mainCamera.transform.position.z;
            float height = 2 * mainCamera.orthographicSize;
            float width = height * Screen.width / Screen.height;

            RectangleF cameraBounds = new RectangleF(x, y, width, height);
            return cameraBounds;
        }

        public void SetMainCameraSize(int width, int height)
        {
            // Check aspect ratio
            float currentAspect = Screen.width / Screen.height;
            //float desiredAspect = width / (float)height;

            int calcWidth = (int)(height * Screen.width / Screen.height);
            if(calcWidth != width)
            {
                Debug.Log(string.Format("A desired camera view of {0} by {1} doesn't match up with the current screen ratio of {3}", width, height, currentAspect));
                return;
            }

            mainCamera.orthographicSize = height / 2;
        }

        public void KeepLevelInMainView(ref float x, ref float y)
        {
            RectangleF mainCameraBounds = GetMainCameraViewBounds();
            float viewWidth = mainCameraBounds.Width;
            float viewHeight = mainCameraBounds.Height;

            float levelX = currentLevel.transform.position.x;
            float levelY = currentLevel.transform.position.z;
            int levelWidth = currentLevel.length;
            int levelHeight = currentLevel.width;

            float minX = (viewWidth - levelWidth) / 2f + levelX;
            float maxX = (levelWidth - viewWidth) / 2f + levelX;

            float minY = (viewHeight - levelHeight) / 2f + levelY;
            float maxY = (levelHeight - viewHeight) / 2f + levelY;

            x = Mathf.Clamp(x, minX, maxX);
            y = Mathf.Clamp(y, minY, maxY);
        }

        public void KeepLevelInMainView(float x, float y, ref float deltaX, ref float deltaY)
        {
            float finalX = x + deltaX;
            float finalY = y + deltaY;

            RectangleF mainCameraBounds = GetMainCameraViewBounds();
            float viewWidth = mainCameraBounds.Width;
            float viewHeight = mainCameraBounds.Height;

            float levelX = currentLevel.transform.position.x;
            float levelY = currentLevel.transform.position.z;
            int levelWidth = currentLevel.length;
            int levelHeight = currentLevel.width;

            float minX = (viewWidth - levelWidth) / 2f + levelX;
            float maxX = (levelWidth - viewWidth) / 2f + levelX;

            float minY = (viewHeight - levelHeight) / 2f + levelY;
            float maxY = (levelHeight - viewHeight) / 2f + levelY;

            deltaX = Mathf.Clamp(finalX, minX, maxX) - x;
            deltaY = Mathf.Clamp(finalY, minY, maxY) - y;
        }
    }
}