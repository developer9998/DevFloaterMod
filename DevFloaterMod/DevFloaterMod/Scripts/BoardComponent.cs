using UnityEngine;
using System.Collections;
using DevFloaterMod.Models;

namespace DevFloaterMod.Scripts
{
    public class BoardComponent : MonoBehaviour
    {
        public DivingBoard board;
        public float adjustor;

        /// <summary>
        /// Called whenever this component is created
        /// </summary>
        public void Start()
        {
            // Prepares a detector for the board
            board.divingBoardCollider.gameObject.AddComponent<BoardDetector>().boardComp = this;

            // Starts a corutine loop for animating the diving board
            StartCoroutine(AnimationLoop());
        }

        /// <summary>
        /// Animates the board bouncing with a certain float operator
        /// </summary>
        /// <param name="force">The float value</param>
        public void BounceObjectQueue(float force) => StartCoroutine(OnBounce(force));

        /// <summary>
        /// The local method used for animating the board
        /// </summary>
        private IEnumerator OnBounce(float force)
        {
            board.divingBoardSource.Play();
            adjustor = -force * 5f;
            yield return new WaitForSeconds(force * 0.04f);
            for (int i = 0; i < 2; i++)
            {
                adjustor = -1.25f;
                yield return new WaitForSeconds(force * 0.05f);
                adjustor = 1.25f;
                yield return new WaitForSeconds(force * 0.025f);
            }
            adjustor = 0;
            yield break;
        }

        /// <summary>
        /// The coroutine used for animating the diving board
        /// </summary>
        IEnumerator AnimationLoop()
        {
            while (true)
            {
                foreach(var transformObject in board.divingBoardTransformList)
                {
                    transformObject.transform.localRotation = Quaternion.Lerp(transformObject.transform.localRotation, Quaternion.Euler(adjustor, 0, 0), 0.08f);
                }
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
