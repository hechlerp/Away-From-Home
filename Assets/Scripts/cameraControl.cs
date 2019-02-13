using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.mousePosition[0] < 80) {
            transform.position += new Vector3(-10 * Time.deltaTime, 0, 0);
        }
        if (Input.mousePosition[0] > Screen.width - 80)
        {
            transform.position += new Vector3(10 * Time.deltaTime, 0, 0);
        }
        if (Input.mousePosition[1] < 80)
        {
            transform.position += new Vector3(0, -10 * Time.deltaTime, 0);
        }
        if (Input.mousePosition[1] > Screen.height - 80)
        {
            transform.position += new Vector3(0, 10 * Time.deltaTime, 0);
        }
    }
}
