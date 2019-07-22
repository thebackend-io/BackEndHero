using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowController : MonoBehaviour {

	private const float MAXIMUM_PULL = 1.0f;

    public Transform aimPreviewT;
	public Player player;

	Vector3 inputStartPos;
	Vector3 inputEndPos;

    private void Start()
    {
        MobileInput.Instance.SetPlayer(player);
    }
    
    private void Update()
    {
		if (player.isLive && player.isGrounded) {
			PoolInput();
		} else {
			aimPreviewT.gameObject.SetActive(false);
		}
    }

	// ==================================================
    private void PoolInput()
    {
        Vector3 sd = MobileInput.Instance.swipeDelta;
        if (Mathf.Approximately(sd.x, 0.0f) && Mathf.Approximately(sd.y, 0.0f)) {
            sd = new Vector3(0, 10f, 0);
        }
		sd.Set(sd.x, sd.y, sd.z);

        if (MobileInput.Instance.hold)
        {
            aimPreviewT.gameObject.SetActive(true);
        }
        else
        {
            aimPreviewT.gameObject.SetActive(false);
        }
        aimPreviewT.parent.up = sd.normalized;
        aimPreviewT.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(1, 2, 1), sd.magnitude / MAXIMUM_PULL);
        if (MobileInput.Instance.release)
        {
            Vector3 throwVelocity = sd.normalized * 26.0f;
            player.Throw(throwVelocity);
        }
    }
}
