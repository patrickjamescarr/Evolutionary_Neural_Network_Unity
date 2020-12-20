using UnityEngine;

public class CameraController : MonoBehaviour
{
    private readonly float panSpeed = 10f;
    private Vector2 panLimit = new Vector2(40, 50);
    private readonly float scrollSpeed = 50f;
    private Camera cam;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // Pan
        var pos = transform.position;

        if (Input.GetKey("w"))
        {
            pos.y += panSpeed * Time.unscaledDeltaTime;
        }

        if (Input.GetKey("s"))
        {
            pos.y -= panSpeed * Time.unscaledDeltaTime;
        }

        if (Input.GetKey("d"))
        {
            pos.x += panSpeed * Time.unscaledDeltaTime;
        }

        if (Input.GetKey("a"))
        {
            pos.x -= panSpeed * Time.unscaledDeltaTime;
        }

        //pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        //pos.y = Mathf.Clamp(pos.y, -panLimit.y, panLimit.y);

        transform.position = pos;

        // Zoom
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * scrollSpeed * Time.unscaledDeltaTime;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 5, 50);
    }
}
