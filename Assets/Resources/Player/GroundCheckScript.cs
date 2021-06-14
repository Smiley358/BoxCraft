using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheckScript : MonoBehaviour
{
    private GameObject Parent;
    private bool IsGround;

    void Start()
    {
        Parent = transform.parent.gameObject;
        IsGround = false;
    }

    public void OnTriggerEnter(UnityEngine.Collider other)
    {
        Parent.SendMessage("OnTheGround");
        IsGround = true;
    }

    public void OnTriggerExit(UnityEngine.Collider other)
    {
        Parent.SendMessage("Floating");
        IsGround = false;
    }

    public void OnTriggerStay(UnityEngine.Collider other)
    {
        //‰½‰ñ‚àSendMessageŒÄ‚Ñ‚½‚­‚È‚¢‚Ì‚Å
        if (!IsGround)
        {
            Parent.SendMessage("OnTheGround");
            IsGround = true;
        }
    }
}
