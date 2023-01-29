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

namespace DevFloaterMod.FloaterPhysics
{
    public readonly struct AverageDirection
    {
        private readonly uint directionAmount;

        private readonly Vector3 vector;

        private readonly float speed;

        public uint Amount
        {
            get
            {
                return directionAmount;
            }
        }

        public float Speed
        {
            get
            {
                if (directionAmount <= 1 || speed == 0f)
                {
                    return speed;
                }

                return speed / (float)directionAmount;
            }
        }

        public Vector3 Direction
        {
            get
            {
                return vector.normalized;
            }
        }

        public Vector3 Vector
        {
            get
            {
                if (directionAmount < 2)
                {
                    return vector;
                }

                return new Vector3((vector.x != 0f) ? (vector.x / (float)directionAmount) : 0f, (vector.y != 0f) ? (vector.y / (float)directionAmount) : 0f, (vector.z != 0f) ? (vector.z / (float)directionAmount) : 0f);
            }
        }

        public static AverageDirection Zero
        {
            get
            {
                return new AverageDirection(Vector3.zero, 0f, 0u);
            }
        }

        public AverageDirection(Vector3 dir, float speedVal, uint dirAmount = 1u)
        {
            vector = dir;
            speed = speedVal;
            directionAmount = dirAmount;
        }

        public static AverageDirection operator +(AverageDirection first, AverageDirection secound)
        {
            return new AverageDirection(first.vector + secound.vector, first.speed + secound.speed, first.directionAmount + 1);
        }

        public static AverageDirection operator -(AverageDirection first, AverageDirection secound)
        {
            return new AverageDirection(first.vector - secound.vector, first.speed - secound.speed, first.directionAmount - 1);
        }
    }
}
