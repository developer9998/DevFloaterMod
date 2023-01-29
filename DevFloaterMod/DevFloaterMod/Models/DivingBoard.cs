using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DevFloaterMod.Models
{
    [Serializable]
    public class DivingBoard
    {
        public GameObject divingBoardObject;
        public BoxCollider divingBoardCollider;
        public AudioSource divingBoardSource;
        public List<Transform> divingBoardTransformList = new List<Transform>();

        /// <summary>
        /// Prepares the diving board for animation
        /// </summary>
        public void Setup()
        {
            Transform origin = divingBoardObject.transform.Find("Armature");
            Transform lastObject = null;
            for (int i = 0; i < 7; i++)
            {
                // Sets the last object to either the first transform it can find in the armature, or the object in can find in the last object we looked at if it isn't null
                if (lastObject == null) lastObject = origin.GetChild(0);
                else lastObject = lastObject.GetChild(0);

                // Adds the object to the armature list used to animate the diving board.
                divingBoardTransformList.Add(lastObject);
            }
        }

        /// <summary>
        /// Sets the activity of the board to a bool operator
        /// </summary>
        /// <param name="active">The bool value</param>
        public void SetActive(bool active)
        {
            divingBoardCollider.enabled = active;
            divingBoardObject.SetActive(active);
            divingBoardSource.Stop();
        }
    }
}
