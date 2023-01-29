/*
The MIT License (MIT)

Copyright © 2021 AHauntedArmy

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), 
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/


using UnityEngine;
using UnityEngine.XR;
using DevFloaterMod.FloaterPhysics;

namespace DevFloaterMod.Models
{
    public struct InputState
    {
        public bool isActive;
        public bool wasPressed;
        public bool wasReleased;
    }

    public abstract class Hand : MonoBehaviour
    {
        private float speed;

        private Vector3 lastPosition = Vector3.zero;

        private Vector3 direction = Vector3.zero;

        private Vector3 rawDirection = Vector3.zero;

        private static Transform bodyOffset;

        private AverageDirection smoothedDirection = AverageDirection.Zero;

        private InputDevice controller;

        private InputState triggerButton;
        private InputState gripButton;

        protected abstract XRNode controllerNode { get; }

        public float Speed => speed;

        public Vector3 Direction => direction;

        public Vector3 RawDirection => rawDirection;

        public InputDevice inputDevice
        {
            get
            {
                if (!controller.isValid)
                {
                    return controller = InputDevices.GetDeviceAtXRNode(controllerNode);
                }

                return controller;
            }
        }

        public InputState TriggerButton => triggerButton;

        public InputState GripButton => gripButton;

        public void Awake()
        {
            if (bodyOffset == null)
            {
                bodyOffset = GorillaLocomotion.Player.Instance.turnParent.gameObject.transform;
            }
        }

        internal void OnEnable()
        {
            lastPosition = gameObject.transform.position - bodyOffset.position;
        }

        internal void OnDisable()
        {
            lastPosition = Vector3.zero;
            rawDirection = Vector3.zero;
            direction = Vector3.zero;
            speed = 0f;
            smoothedDirection = AverageDirection.Zero;
        }

        internal void Update()
        {
            UpdateButtonState(CommonUsages.triggerButton, ref triggerButton);
            UpdateButtonState(CommonUsages.gripButton, ref gripButton);
            inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out var _);
        }

        internal void LateUpdate()
        {
            Vector3 vector = gameObject.transform.position - bodyOffset.position;
            rawDirection = lastPosition - vector;
            lastPosition = vector;
            speed = 0f;
            if (triggerButton.wasPressed && gripButton.wasPressed)
            {
                speed = rawDirection.magnitude;
                speed = speed > 0f ? speed / Time.deltaTime : 0f;
                smoothedDirection += new AverageDirection(rawDirection, 0f);
                direction = rawDirection.normalized;
            }
            else if (triggerButton.isActive && gripButton.isActive)
            {
                speed = rawDirection.magnitude;
                speed = speed > 0f ? speed / Time.deltaTime : 0f;
                smoothedDirection += new AverageDirection(rawDirection * 0.5f, 0f);
                direction = smoothedDirection.Vector.normalized;
            }
            else if (triggerButton.wasReleased || gripButton.wasReleased)
            {
                speed = 0f;
                smoothedDirection = AverageDirection.Zero;
                direction = Vector3.zero;
            }
        }

        private void UpdateButtonState(InputFeatureUsage<bool> button, ref InputState buttonState)
        {
            bool value = false;
            inputDevice.TryGetFeatureValue(button, out value);
            if (value)
            {
                if (!buttonState.isActive)
                {
                    buttonState.wasPressed = true;
                }
                else
                {
                    buttonState.wasPressed = false;
                }
            }
            else if (buttonState.isActive)
            {
                buttonState.wasReleased = true;
                buttonState.wasPressed = false;
            }
            else
            {
                buttonState.wasReleased = false;
                buttonState.wasPressed = false;
            }

            buttonState.isActive = value;
        }
    }
}
