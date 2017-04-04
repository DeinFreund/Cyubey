using UnityEngine;
using System.Collections;

public class VelocityInterpolator : MonoBehaviour {

    Vector3 velocity = new Vector3();

    public void updateTransform(Vector3 pos, Quaternion rot, Vector3 vel)
    {
        var fac = Mathf.Min(1f, Vector3.Distance(transform.position, pos) / 10);
        transform.position = transform.position * (1 - fac) + fac * pos;
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, 0.5f);

        velocity = vel;
    }


	void Start () {
	
	}
	
	void Update () {
        transform.position += velocity * Time.deltaTime;
	}
}
