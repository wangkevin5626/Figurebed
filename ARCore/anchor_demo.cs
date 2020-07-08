
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

public class AnchorManager : MonoBehaviour
{
    public GameObject anchoredPrefab;
    private bool mIsQuitting = false;

    Anchor anchor;
    Vector3 lastAnchoredPosition;
    Quaternion lastAnchorRotation;

    void Start()
    {
        OnCheckDevice();
    }


    // Update is called once per frame
    void Update()
    {
        UpdateApplicationLifecycle();

        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.Default;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            anchor = Session.CreateAnchor(hit.Pose);
            GameObject.Instantiate(anchoredPrefab, hit.Pose.position, hit.Pose.rotation);
            lastAnchoredPosition = hit.Pose.position;
            lastAnchorRotation = hit.Pose.rotation;

            string tipStr = "frame x = " + Frame.Pose.position.x.ToString();
            tipStr += " ,frame y = ";
            tipStr += Frame.Pose.position.x.ToString();
            tipStr += " ,frame z = ";
            tipStr += Frame.Pose.position.z.ToString();
            tipStr += " ,frame rotation = ";
            tipStr += Frame.Pose.rotation.w.ToString();
            ShowAndroidToastMessage(tipStr);
        }

    }

    /// <summary>
    /// 检查设备
    /// </summary>
    private void OnCheckDevice()
    {
        if (Session.Status == SessionStatus.ErrorSessionConfigurationNotSupported)
        {
            ShowAndroidToastMessage("ARCore在本机上不受支持或配置错误！");
            mIsQuitting = true;
            Invoke("DoQuit", 0.5f);
        }
        else if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            ShowAndroidToastMessage("AR应用的运行需要使用摄像头，现无法获取到摄像头授权信息，请允许使用摄像头！");
            mIsQuitting = true;
            Invoke("DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            ShowAndroidToastMessage("ARCore运行时出现错误，请重新启动本程序！");
            mIsQuitting = true;
            Invoke("DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// 管理应用的生命周期
    /// </summary>
    private void UpdateApplicationLifecycle()
    {
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (mIsQuitting)
        {
            return;
        }
    }

    /// <summary>
    /// 退出程序
    /// </summary>
    private void DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// 弹出信息提示
    /// </summary>
    /// <param name="message">要弹出的信息</param>
    private void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
