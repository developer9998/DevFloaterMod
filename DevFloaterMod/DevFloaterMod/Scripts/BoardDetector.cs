using UnityEngine;

namespace DevFloaterMod.Scripts
{
    public class BoardDetector : MonoBehaviour
    {
        public BoardComponent boardComp;

        public void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.name == "Body Collider")
            {
                if (boardComp.adjustor == 0)
                {
                    boardComp.BounceObjectQueue(2.5f);
                    Plugin.Instance.body.AddForce(new Vector3(0f, -Plugin.Instance.body.velocity.y * 8, 0f), ForceMode.VelocityChange);
                    Plugin.Instance.body.AddForce(GorillaLocomotion.Player.Instance.bodyCollider.transform.forward * 4, ForceMode.VelocityChange);
                }
            }
        }
    }
}
