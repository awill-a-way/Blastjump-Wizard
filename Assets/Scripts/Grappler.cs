using UnityEngine;

public class Grappler : MonoBehaviour
{
    public bool holdingSomething;
    private GameObject heldObject;
    public Camera playerCamera;
    public Transform holdPoint;

    void Update()
    {   

    }

    public void Grab()
    {
        if (heldObject == null)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, 2f))
            {
                var target = hit.collider.gameObject;
                var grappleable = target.GetComponent<Grappleable>();
                
                if (grappleable != null && grappleable.grappled == false && grappleable.canBeGrappled == true)
                {
                    heldObject = target;
                    grappleable.grappled = true;

                    if (grappleable.c != null) grappleable.c.enabled = false;
                    if (grappleable.rb != null)
                    {
                        grappleable.rb.useGravity = false;
                        grappleable.rb.isKinematic = true;
                    }

                    heldObject.transform.SetParent(holdPoint, true);
                    heldObject.transform.localPosition = Vector3.zero;

                    holdingSomething = true;

                    Debug.Log("Grabbed "+heldObject+"!");
                }
            }
            else
            {
                Debug.Log("Couldn't grab anything!");
            }
        }
    }

    public void Throw()
    {
        if (heldObject != null)
        {
            heldObject.transform.SetParent(null, true);

            var grappleable = heldObject.GetComponent<Grappleable>();
            grappleable.grappled = false;
            if (grappleable.c != null) grappleable.c.enabled = true;
            if (grappleable.rb != null)
            {
                grappleable.rb.useGravity = grappleable._originalRigidbodyUseGravity;
                grappleable.rb.isKinematic = grappleable._originalRigidbodyIsKinematic;
            }

            grappleable.ResetCanBeGrappled(0f);
            Debug.Log("Threw "+heldObject+"!");
            heldObject = null;
            holdingSomething = false;
        }
    }
}
