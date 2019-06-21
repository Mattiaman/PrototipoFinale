using GoogleARCore;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeController : MonoBehaviour
{

	public Camera camera;

	public int frameLimit=1;

	public GameObject DetectedPlanePrefab;

	private List<DetectedPlane> planes = new List<DetectedPlane>();

	private bool isQuitting = false;

	public GameObject prefab;

	private const float k_ModelRotation = 270.0f;

	private int currentFrameOnScene = 0;

	public void Update()
	{
		EventDetection();

		Session.GetTrackables<DetectedPlane>(planes);
		bool trackingUI = true;
		for (int i = 0; i < planes.Count; i++)
		{
			if (planes[i].TrackingState == TrackingState.Tracking)
			{
				trackingUI = false;
				break;
			}
		}

		Touch touch;
		if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
		{
			return;
		}

		TrackableHit hit;
		TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
			TrackableHitFlags.FeaturePointWithSurfaceNormal;
		RaycastHit[] h;
		Ray r = camera.ScreenPointToRay(touch.position);
		h = Physics.RaycastAll(r);
		for(int i=0;i<h.Length;i++)
			h[i].collider.gameObject.SendMessage("turnOnGravity", SendMessageOptions.DontRequireReceiver);
		if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
		{
			if ((hit.Trackable is DetectedPlane) &&
				Vector3.Dot(camera.transform.position - hit.Pose.position,
					hit.Pose.rotation * Vector3.up) < 0)
			{
				Debug.Log("Hit at back of the current DetectedPlane");
			}
			else
			{
				if ((hit.Trackable is DetectedPlane)){
					DetectedPlane plane=(DetectedPlane)hit.Trackable;
					if (plane.PlaneType == DetectedPlaneType.Vertical && currentFrameOnScene<frameLimit)
					{

						var andyObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);
						andyObject.transform.position = new Vector3(hit.Pose.position.x + 0.06f, hit.Pose.position.y + 0.06f, hit.Pose.position.z + 0.06f);

						var placedObjectForward = Vector3.up;
						var placedObjectUp = plane.CenterPose.rotation * Vector3.up;
						Vector3 normal = plane.CenterPose.rotation * Vector3.up;

						andyObject.transform.rotation = Quaternion.LookRotation(placedObjectForward, placedObjectUp);
						andyObject.transform.Rotate(-90, 0, 180);
                        andyObject.transform.Translate(Vector3.forward*0.05f);

						var anchor = hit.Trackable.CreateAnchor(hit.Pose);

						andyObject.transform.parent = anchor.transform;

						currentFrameOnScene++;

						if (currentFrameOnScene >= frameLimit)
						{
							List<GameObject> l = DetectedPlanePrefab.GetComponent<GoogleARCore.Examples.Common.DetectedPlaneGenerator>().getPlaneList();
							for (int i = 0; i < l.Count; i++)
								Destroy(l[i].GetComponent<MeshRenderer>());
						}
					}
					else
					{
						Handheld.Vibrate();
					}
					
				}

				
			}
		}

	}

	private void EventDetection()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}
		if (Session.Status != SessionStatus.Tracking)
		{
			const int trackingSleep = 15;
			Screen.sleepTimeout = trackingSleep;
		}
		else
		{
			//_ShowAndroidToastMessage("tracking");
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}
		if (isQuitting)
		{
			return;
		}

		if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
		{
			_ShowAndroidToastMessage("Camera permission is needed to run this application.");
			isQuitting = true;
			Invoke("_DoQuit", 0.5f);
		}
		else if (Session.Status.IsError())
		{
			_ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
			isQuitting = true;
			Invoke("_DoQuit", 0.5f);
		}
	}

	private void _ShowAndroidToastMessage(string message)
	{
		AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		if (unityActivity != null)
		{
			AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
			unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
			{
				AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
				message, 0);
				toastObject.Call("show");
			}));
		}
	}
}
