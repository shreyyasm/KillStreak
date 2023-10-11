using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
	public TextMeshProUGUI FpsText;

	private float pollingTime = 0.1f;
	private float time;
	private int frameCount;


	void Update()
	{
		//Application.targetFrameRate = 60;
		// Update time.
		time += Time.deltaTime;

		// Count this frame.
		frameCount++;

		if (time >= pollingTime)
		{
			// Update frame rate.
			int frameRate = Mathf.RoundToInt((float)frameCount / time);
			FpsText.text =  "fps: " + frameRate.ToString();

			// Reset time and frame count.
			time -= pollingTime;
			frameCount = 0;
		}
	}
}