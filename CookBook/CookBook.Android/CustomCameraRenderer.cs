/*using System;
using Android.Content;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;

namespace CookBook.Droid
{
    public class CustomCameraRenderer
    {
        private Handler handler;

        public CustomCameraRenderer(Context context)
        {
            CameraManager cameraManager = (CameraManager)context.GetSystemService(Context.CameraService);
            cameraManager.OpenCamera("cameraId", new CameraCallback(), handler);
        }
    }

    internal class CameraCallback : CameraDevice.StateCallback
    {
        public override void OnDisconnected(CameraDevice camera)
        {
        }

        public override void OnError(CameraDevice camera, [GeneratedEnum] CameraError error)
        {
        }

        public override void OnOpened(CameraDevice camera)
        {
        }
    }
}
*/