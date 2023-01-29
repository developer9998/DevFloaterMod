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

using BepInEx;
using DevFloaterMod.Models;
using DevFloaterMod.FloaterPatches;
using DevFloaterMod.FloaterPhysics;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Photon.Pun;
using GorillaLocomotion;
using UnityEngine;
using Utilla;
using System.Collections.Generic;
using DevFloaterMod.Scripts;

namespace DevFloaterMod
{
    [ModdedGamemode]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; internal set; }

        public bool Init = false;
        public bool InModdedRoom;
        public bool underwater;
        public bool handUnderwater;

        internal Arm leftArm;
        internal Arm rightArm;

        internal GameObject waterObject;
        internal GameObject waterUIObject;
        internal List<DivingBoard> boards = new List<DivingBoard>();

        internal Vector3 lastGravity = Vector3.zero;
        internal GameObject waterReferenceObject;
        internal AudioClip waterSway;
        internal AudioSource waterSource;
        internal AssetBundle bundle;
        internal GameObject smallWater;
        internal GameObject bigWater;
        internal List<AudioClip> sounds = new List<AudioClip>();

        /* Water Mechanics */
        internal Rigidbody body;
        internal LeftHandTracker leftHand;
        internal RightHandTracker rightHand;

        internal float acceleration = 2.5f;
        internal float maxSpeed = 7f;
        internal float lastLeftSpeed;
        internal float lastRightSpeed;
        internal bool leftHandWater;
        internal bool rightHandWater;
        internal bool handMovementLeft;
        internal bool handMovementRight;
        internal bool insertedLeft;
        internal bool insertedRight;
        internal Vector3 AccelVector = Vector3.zero;
        internal float waterLevel = 8.7f;

        /* Water Animation */
        internal float speed = 0.002f;

        internal void Awake()
        {
            Instance = this;

            HarmonyPatches.ApplyHarmonyPatches();
        }

        public void OnInitialized()
        {
            if (Init) return;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DevFloaterMod.Resources.floaterbundle");
            bundle = AssetBundle.LoadFromStream(stream);

            /* Sounds */
            waterSway = bundle.LoadAsset<AudioClip>("watersway");
            sounds.Add(bundle.LoadAsset<AudioClip>("water1"));
            sounds.Add(bundle.LoadAsset<AudioClip>("water2"));
            sounds.Add(bundle.LoadAsset<AudioClip>("swim1"));
            sounds.Add(bundle.LoadAsset<AudioClip>("swim2"));
            sounds.Add(bundle.LoadAsset<AudioClip>("swim3"));

            /* Particle Effects */
            bigWater = bundle.LoadAsset<GameObject>("HugeWaterBlast");
            smallWater = bundle.LoadAsset<GameObject>("SmallWaterBlast");

            /* Water Objects*/
            waterObject = Instantiate(bundle.LoadAsset<GameObject>("WaterAbovePrefab"));
            waterObject.transform.localPosition = new Vector3(0, waterLevel, 0);
            waterObject.transform.localEulerAngles = Vector3.zero;
            waterObject.transform.localScale = Vector3.one;

            waterUIObject = Instantiate(bundle.LoadAsset<GameObject>("WaterInsidePrefab"));
            waterUIObject.transform.SetParent(GorillaTagger.Instance.offlineVRRig.headMesh.transform, false);
            waterUIObject.transform.localPosition = Vector3.zero;
            waterUIObject.transform.localEulerAngles = Vector3.zero;
            waterUIObject.transform.localScale = Vector3.one;
            waterUIObject.SetActive(false);

            /* Floaters <333 */
            leftArm = new Arm();
            leftArm.armObject = GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent.parent.gameObject;
            leftArm.floaterObject = Instantiate(bundle.LoadAsset<GameObject>("FloaterFix"));

            leftArm.leftArm = true;

            leftArm.floaterObject.transform.SetParent(leftArm.armObject.transform, false);
            leftArm.floaterObject.transform.localPosition = new Vector3(-0.01555197f, 0.1632531f, -0.009236247f);
            leftArm.floaterObject.transform.localRotation = Quaternion.Euler(-95.67f, -126.618f, -32.797f);
            leftArm.floaterObject.transform.localScale = new Vector3(16.4027f, 16.4027f, 21.40025f);

            rightArm = new Arm();
            rightArm.armObject = GorillaTagger.Instance.offlineVRRig.rightHandTransform.parent.parent.gameObject;
            rightArm.floaterObject = Instantiate(bundle.LoadAsset<GameObject>("FloaterFix"));
  
            rightArm.leftArm = false;

            rightArm.floaterObject.transform.SetParent(rightArm.armObject.transform, false);
            rightArm.floaterObject.transform.localPosition = new Vector3(0.0195077f, 0.154403f, -0.01722785f);
            rightArm.floaterObject.transform.localRotation = Quaternion.Euler(97.42599f, -44.30499f, -28.34601f);
            rightArm.floaterObject.transform.localScale = new Vector3(16.4027f, 16.4027f, -21.40025f);

            /* References for later */
            lastGravity = Physics.gravity;
            leftHand = Player.Instance.leftHandFollower.gameObject.AddComponent<LeftHandTracker>();
            leftHand.enabled = true;
            rightHand = Player.Instance.rightHandFollower.gameObject.AddComponent<RightHandTracker>();
            rightHand.enabled = true;
            body = Traverse.Create(Player.Instance).Field("playerRigidBody").GetValue<Rigidbody>();
            waterReferenceObject = new GameObject();

            // This object is for particle placement and volume measurement
            waterReferenceObject.name = "WaterSoundRef";
            waterSource = waterObject.GetComponent<AudioSource>();
            waterSource.volume = 0;

            // Disabling this stuff now so it wouldn't prevent any issues
            waterObject.SetActive(false);
            Init = true;

            /* Diving boards */
            SpawnDivingBoard(-64.65f, 21.37f, -79.82f, 14.74f);
            SpawnDivingBoard(-67.999f, 21.035f, -60.503f, 82.437f);
            SpawnDivingBoard(-50.87f, 13.8817f, -62.965f, -76.524f);
            SpawnDivingBoard(-75.507f, 9.165397f, -82.225f, 21.246f);
            SpawnDivingBoard(-59.43f, 17.708f, -44.767f, 156.968f);
            SpawnDivingBoard(-109.134f, 17.892f, -127.818f, 87.31901f);
            SpawnDivingBoard(-94.045f, 14.715f, -162.26f, -52.321f);
            SpawnDivingBoard(-118.8063f, 26.535f, -148.0306f, 42.492f);
            SpawnDivingBoard(-99.57237f, 20.562f, -109.8633f, 221.078f);
            SpawnDivingBoard(-9.639f, 32.249f, -91.914f, 399.191f);
            SpawnDivingBoard(27.779f, 22.302f, -104.869f, 379.081f);
            SpawnDivingBoard(67.33221f, 5.894f, -99.219f, 317.407f);
            SpawnDivingBoard(26.307f, 0.467f, -38.78f, 140.467f);
            SpawnDivingBoard(61.55f, 7.398f, -87.94f, 3.851f);

            // Disables all the diving board assets
            foreach(var dBoard in boards) dBoard.SetActive(false);

            InvokeRepeating("RandomizeWaterLevel", 20, 20);
        }

        /// <summary>
        /// Checks a position to see if it's underwater, returns a bool operator if it is or isn't under the water
        /// </summary>
        /// <param name="pos">The postion to check</param>
        /// <returns>A bool operator</returns>
        internal bool IsPositionUnderwater(Vector3 pos)
        {
            if (waterObject != null)
            {
                return pos.y <= waterObject.transform.position.y;
            }

            return false;
        }

        /// <summary>
        /// I think you know how this works
        /// </summary>
        internal void FixedUpdate()
        {
            if (!Init) return;
            waterReferenceObject.transform.position = new Vector3(Player.Instance.headCollider.transform.position.x, waterObject.transform.position.y, Player.Instance.headCollider.transform.position.z);

            // Depending if the player is in or isn't in a room, set the floater material to a VRRig's material, either the OfflineVRRig or the player's server rig
            if (leftArm != null && rightArm != null)
            {
                if (!PhotonNetwork.InRoom)
                {
                    leftArm.floaterObject.GetComponent<Renderer>().material = GorillaTagger.Instance.offlineVRRig.materialsToChangeTo[GorillaTagger.Instance.offlineVRRig.setMatIndex];
                    rightArm.floaterObject.GetComponent<Renderer>().material = GorillaTagger.Instance.offlineVRRig.materialsToChangeTo[GorillaTagger.Instance.offlineVRRig.setMatIndex];
                }
                else
                {
                    leftArm.floaterObject.GetComponent<Renderer>().material = GorillaTagger.Instance.offlineVRRig.materialsToChangeTo[GorillaTagger.Instance.myVRRig.setMatIndex];
                    rightArm.floaterObject.GetComponent<Renderer>().material = GorillaTagger.Instance.offlineVRRig.materialsToChangeTo[GorillaTagger.Instance.myVRRig.setMatIndex];
                }
            }

            if (InModdedRoom)
            {
                bool lastUnderwater = underwater;
                bool lastleftHandWater = leftHandWater;
                bool lastrightHandWater = rightHandWater;
                bool lastMovementLeft = handMovementLeft;
                bool lastMovementRight = handMovementRight; 
                leftHandWater = IsPositionUnderwater(Player.Instance.leftHandFollower.transform.position);
                rightHandWater = IsPositionUnderwater(Player.Instance.rightHandFollower.transform.position);
                underwater = IsPositionUnderwater(Player.Instance.headCollider.transform.position);
                handUnderwater = IsPositionUnderwater(Player.Instance.leftHandFollower.transform.position) || IsPositionUnderwater(Player.Instance.rightHandFollower.transform.position);

                waterSource.volume = 0.5f - (Vector3.Distance(Player.Instance.headCollider.transform.position, waterReferenceObject.transform.position) / 50);

                // If the player isn't fully underwater, generate some funny particle effects for when their hands take a dip inside
                if (!underwater)
                {
                    if (leftHandWater != lastleftHandWater && leftHandWater)
                    {
                        GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.taggedHapticStrength, 0.05f);
                        Splash(WaterSize.Small, new Vector3(Player.Instance.leftHandFollower.transform.position.x, waterObject.transform.position.y, Player.Instance.leftHandFollower.transform.position.z));
                    }
                    if (rightHandWater != lastrightHandWater && rightHandWater)
                    {
                        GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.taggedHapticStrength, 0.05f);
                        Splash(WaterSize.Small, new Vector3(Player.Instance.rightHandTransform.transform.position.x, waterObject.transform.position.y, Player.Instance.rightHandTransform.transform.position.z));
                    }
                }

                if (underwater != lastUnderwater)
                {
                    if (underwater)
                    {
                        Physics.gravity = lastGravity * 0.1f;
                        Splash(WaterSize.Large, waterReferenceObject.transform.position);
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(sounds[Random.Range(2, 4)]);
                        Physics.gravity = lastGravity;
                    }
                }

                // If the player is underwater, do all the cool physics stuff
                if (underwater)
                {
                    body.velocity += AccelVector;
                    float leftHandSpeed = acceleration * (leftHand.Speed * 0.01f);
                    float rightHandSpeed = acceleration * (rightHand.Speed * 0.01f);

                    AccelVector = Vector3.zero;
                    handMovementLeft = leftHand.Speed >= 0.35f;
                    handMovementRight = rightHand.Speed >= 0.35f;

                    if (leftHandSpeed == 0) insertedLeft = false;
                    if (rightHandSpeed == 0) insertedRight = false;

                    if (leftHandSpeed > 0f || rightHandSpeed > 0f)
                    {
                        Vector3 lookAssist = Camera.main.transform.forward * 0.2f;
                        AccelVector += (rightHand.Direction + lookAssist).normalized * rightHandSpeed;
                        AccelVector += (leftHand.Direction + lookAssist).normalized * leftHandSpeed;

                        if (leftHandSpeed > 0f) GorillaTagger.Instance.StartVibration(true, leftHandSpeed / 3, Time.fixedDeltaTime);
                        if (rightHandSpeed > 0f) GorillaTagger.Instance.StartVibration(false, rightHandSpeed / 3, Time.fixedDeltaTime);

                        if (body.velocity.magnitude > maxSpeed || (body.velocity + AccelVector).magnitude > maxSpeed)
                        {
                            body.velocity = (body.velocity + AccelVector).normalized * maxSpeed;
                            AccelVector = Vector3.zero;
                        }
                    }
                }

                if (lastMovementLeft != handMovementLeft && handMovementLeft && !insertedLeft)
                {
                    insertedLeft = true;
                    GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(sounds[Random.Range(2, 4)]);
                }
                if (lastMovementRight != handMovementRight && handMovementRight && !insertedRight)
                {
                    insertedRight = true;
                    GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(sounds[Random.Range(2, 4)]);
                }

                // Lerp the water object to the water level slowly
                waterObject.transform.localPosition = Vector3.Lerp(waterObject.transform.localPosition, new Vector3(0, waterLevel, 0), 0.008f);
                waterObject.SetActive(true);

                waterUIObject.SetActive(underwater);

                foreach(MeshRenderer renderer in waterObject.GetComponentsInChildren<MeshRenderer>()) renderer.material.mainTextureOffset += new Vector2(speed * 1.5f, speed * 0.25f / 2);
                foreach(ParticleSystem sys in waterObject.GetComponentsInChildren<ParticleSystem>())
                {
                    if (Vector3.Distance(sys.transform.position, Player.Instance.headCollider.transform.position) <= 25) sys.enableEmission = true;
                    else sys.enableEmission = false;
                }
            }
            else
            {
                waterObject.SetActive(false);
                waterUIObject.SetActive(false);
                // waterSource is a component in waterObject so we don't need to do anything about that here
            }
        }

        public void Splash(WaterSize size, Vector3 impact)
        {
            if (size == WaterSize.Large)
            {
                GameObject wuhUh = Instantiate(bigWater);
                wuhUh.transform.position = impact;
                GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(sounds[Random.Range(0, 1)]);
                Destroy(wuhUh, 5);
            }
            else if (size == WaterSize.Small)
            {
                GameObject wuhUh = Instantiate(smallWater);
                wuhUh.transform.position = impact;
                GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(sounds[Random.Range(2, 4)]);
                Destroy(wuhUh, 5);
            }
        }

        /// <summary>
        /// Creates a diving board using a few operators
        /// </summary>
        internal void SpawnDivingBoard(float x, float y, float z, float rotation)
        {
            GameObject mainObj = Instantiate(bundle.LoadAsset<GameObject>("GorillaDiveBoard"));
            mainObj.transform.position = new Vector3(x, y, z);
            mainObj.transform.rotation = Quaternion.Euler(0, rotation, 0);
            mainObj.transform.localScale = Vector3.one * 0.2284465f;

            DivingBoard board = new DivingBoard();
            board.divingBoardObject = mainObj;
            board.divingBoardCollider = mainObj.GetComponentInChildren<BoxCollider>();
            board.divingBoardSource = board.divingBoardCollider.GetComponent<AudioSource>();
            board.divingBoardCollider.isTrigger = true;
            board.divingBoardCollider.gameObject.layer = 9;
            boards.Add(board);
            board.Setup();
            board.divingBoardObject.AddComponent<BoardComponent>().board = board;
        }

        /// <summary>
        /// Sets the water level to a randomized water elevation using the GetRandomElevation method
        /// </summary>
        internal void RandomizeWaterLevel()
        {
            waterLevel = GetRandomElevation();
        }

        /// <summary>
        /// Generates a random water elevation
        /// </summary>
        /// <returns></returns>
        internal float GetRandomElevation()
        {
            float elevation = Random.Range(0, 25);
            if (waterLevel == elevation) return GetRandomElevation();
            return elevation;
        }

        /// <summary>
        /// When the player joins a modded lobby
        /// </summary>
        /// <param name="gamemode">The gamemode</param>
        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            InModdedRoom = true;
            foreach (var dBoard in boards) dBoard.SetActive(true);
        }

        /// <summary>
        /// When the player leaves a modded lobby
        /// </summary>
        /// <param name="gamemode">The gamemode</param>
        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            InModdedRoom = false;
            foreach (var dBoard in boards) dBoard.SetActive(false);
        }
    }
}
