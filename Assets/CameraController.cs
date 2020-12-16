using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float panSpeed = 10f;
    private float panBorderThickness = 20f;
    private Vector2 panLimit = new Vector2(40, 50);
    private float scrollSpeed = 1000f;
    private Camera cam;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        var pos = transform.position;


        //if(Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness)
        //{
        //    pos.y += panSpeed * Time.deltaTime;
        //}

        //if (Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness)
        //{
        //    pos.y -= panSpeed * Time.deltaTime;
        //}

        //if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorderThickness)
        //{
        //    pos.x += panSpeed * Time.deltaTime;
        //}

        //if (Input.GetKey("a") || Input.mousePosition.x <= panBorderThickness)
        //{
        //    pos.x -= panSpeed * Time.deltaTime;
        //}

        if (Input.GetKey("w"))
        {
            pos.y += panSpeed * Time.deltaTime;
        }

        if (Input.GetKey("s"))
        {
            pos.y -= panSpeed * Time.deltaTime;
        }

        if (Input.GetKey("d"))
        {
            pos.x += panSpeed * Time.deltaTime;
        }

        if (Input.GetKey("a"))
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        var scroll = Input.GetAxis("Mouse ScrollWheel");

        cam.orthographicSize -= scroll * scrollSpeed * Time.deltaTime;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 5, 50);
        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, -panLimit.y, panLimit.y);

        transform.position = pos;
    }
}
