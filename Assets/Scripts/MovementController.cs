using UnityEngine;
using System.Collections;

public class MovementController : MonoBehaviour {

    public float speed = 5;
    public float jump = 5;
    public float gravity = 10;
    public Transform playerRotation;


    // Use this for initialization
    void Start() {

    }

    private Vector3 vel;
    private Vector3 lastPos;
    private Vector3[] projections = new Vector3[] { new Vector3(1, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0) , new Vector3(0, 0, 1), new Vector3(0,0,0)};
    private Vector3[] baseVecs = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1)};

    void Update ()
    {
        Vector3 campos = Camera.main.transform.position;
        Position feetpos = new Position((int)System.Math.Ceiling(transform.position.x), (int)System.Math.Ceiling(transform.position.y) - 2, (int)System.Math.Ceiling(transform.position.z) - 1);
        if (feetpos.getChunk() == null)
        {
            Debug.Log("Area not loaded, freezing player");
            return;
        }
        vel *= Mathf.Pow(0.7f, Time.deltaTime * 60);
        vel.y /= Mathf.Pow(0.7f, Time.deltaTime * 60);
        vel.y *= Mathf.Pow(0.996f, Time.deltaTime * 60);
        vel += Time.deltaTime * Vector3.ProjectOnPlane(speed * (playerRotation.right * Input.GetAxis("Horizontal") + playerRotation.forward * Input.GetAxis("Vertical")), Vector3.up);
        //if ((feetpos.below().getBlock()is Air))
        vel.y = vel.y - gravity * Time.deltaTime;

        int tries = 0;
        Vector3 origVel = vel;
        Vector3 temp = Vector3.zero;
        //Debug.Log(vel);
        move:
        lastPos = transform.position;
        transform.position += Time.deltaTime * vel;
        check:

        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>())
        {
            if (!child.gameObject.name.ToLower().Equals("collider")) continue;
            feetpos = new Position((int)System.Math.Ceiling(child.position.x), (int)System.Math.Ceiling(child.position.y) - 2, (int)System.Math.Ceiling(child.position.z) - 1);
            Vector3 accpos = new Vector3((lastPos.x), (lastPos.y) - 2, (lastPos.z) - 1);

            if (!(feetpos.getBlock()is Air))
            {
                if (feetpos.above().getBlock() is Air && child.localPosition.y <= 0.001)
                {
                    transform.position = new Vector3(transform.position.x, feetpos.getY() + 2f, transform.position.z);
                    vel.y = gravity * 0.0f;
                }else
                if (feetpos.above().above().getBlock() is Air && child.localPosition.y <= 0.001)
                {
                    transform.position = new Vector3(transform.position.x, feetpos.getY() + 3f, transform.position.z);
                    vel.y = gravity * 0.0f;
                }
                else
                {

                    Vector3 f = feetpos + new Vector3(0.5f, 0.5f, 0.5f) - accpos;
                    //Debug.DrawRay(transform.position, f, Color.red, 0.1f);
                    if (Mathf.Abs(f.x) >= Mathf.Abs(f.y) && Mathf.Abs(f.x) >= Mathf.Abs(f.z)) f.y = f.z = 0;
                    if (Mathf.Abs(f.y) >= Mathf.Abs(f.x) && Mathf.Abs(f.y) >= Mathf.Abs(f.z)) f.x = f.z = 0;
                    if (Mathf.Abs(f.z) >= Mathf.Abs(f.y) && Mathf.Abs(f.z) >= Mathf.Abs(f.x)) f.y = f.x = 0;

                    //Debug.DrawRay(transform.position, f, Color.blue, 0.1f);
                    //Debug.DrawRay(transform.position, vel, Color.yellow, 0.1f);
                    transform.position = lastPos;
                    if (tries < projections.Length)
                    {
                        bool invalid = false;
                        do
                        {
                            invalid = false;
                            temp = Vector3.zero;
                            foreach (Vector3 basis in baseVecs)
                            {
                                temp += Vector3.Project(origVel, Vector3.Project(projections[tries].normalized, basis));
                                if (Vector3.Project(projections[tries].normalized, basis).magnitude > 0.01 && Vector3.Project(origVel, Vector3.Project(projections[tries].normalized, basis)).magnitude < 0.01) invalid = true;
                            }
                            tries++;
                        } while (invalid);

                        transform.position = lastPos + temp * Time.deltaTime;
                        vel = temp;
                    } else return;

                    goto check;
                }
                if (Input.GetButton("Jump"))
                {
                    vel.y += jump;
                }
            }
        }
        Debug.DrawRay(transform.position, temp);
    }
}
