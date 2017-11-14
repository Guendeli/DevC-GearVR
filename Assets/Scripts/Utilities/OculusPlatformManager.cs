using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;

public class OculusPlatformManager : MonoBehaviour {

    /*
     *          Oculus Platform Manager 
     * this snippet handle oculus API features, like achievements
     * photo sharing, entitlements
     */


    public static OculusPlatformManager Instance;
    public string testAchievementName;

	// Use this for initialization
	void Awake () {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
	}

    void Start()
    {
        Core.Initialize("1836727516367793"); // REPLACE WITH YOUR OWN APP ID: https://dashboard.oculus.com/
    }
    // Update is called once per frame

    #region Helper Methods are done here
    /// <summary>
    /// Unlock achievement by it's string id: https://dashboard.oculus.com/
    /// </summary>
    /// <param name="value"></param>
    public void AchievementUnlock(string value)
    {
        Achievements.Unlock(value);
    }

    /// <summary>
    /// Starts Facebook Photo Sharing service
    /// </summary>
    public void PhotoSharing()
    {
        UnityEngine.Application.CaptureScreenshot("Screenshot.png");
        string screShotPath = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "Screenshot.png");
        Media.ShareToFacebook("Sample Text", screShotPath, MediaContentType.Photo);
    }
    #endregion
}
