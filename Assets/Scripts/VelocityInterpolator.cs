using UnityEngine;
using System.Collections;

public class VelocityInterpolator : MonoBehaviour {

    Vector3 velocity = new Vector3();

    public void updateTransform(Vector3 pos, Quaternion rot, Vector3 vel)
    {
        Vector3 myPos = transform.position - MovementController.worldParent.transform.position;
        var fac = Mathf.Min(1f, Vector3.Distance(myPos, pos) / 10);
        myPos = myPos * (1 - fac) + fac * pos;
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, 0.5f);

        velocity = vel;
        transform.position = myPos + MovementController.worldParent.transform.position;
    }


	void Start () {
	
	}
	
	void Update () {
        transform.position += velocity * Time.deltaTime;
	}
}
