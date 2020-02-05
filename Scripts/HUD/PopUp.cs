using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    public Component component { get; private set; }
    public Animator animator { get; private set; }

    public bool hasTimer = true;
    public float timer = 0f;

    public bool followCam = false;
    [SerializeField] Transform camera = null;

    InGamePhotonManager inGamePhoton;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // Temporaire
        component = GetComponent<Text>();
        if (component == null) component = GetComponent<Image>();
        if (component == null) component = GetComponent<TextMesh>();

        inGamePhoton = InGamePhotonManager.Instance;
    }

    private IEnumerator DelayedSetCameraTransform()
    {
        while (inGamePhoton.localPlayer == null)
        {
            yield return new WaitForEndOfFrame();
        }

        camera = InGamePhotonManager.Instance.localPlayer.GetComponent<PlayerController>().pCamera.Camera.transform;
    }

    void Update()
    {
        if (followCam)
        {
            if (camera == null)
            {
                StartCoroutine("DelayedSetCameraTransform");
            }
            else
            {
                transform.rotation = camera.rotation;
            }

        }

        if (hasTimer)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
