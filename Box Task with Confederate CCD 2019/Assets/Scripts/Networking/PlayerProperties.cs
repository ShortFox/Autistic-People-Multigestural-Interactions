namespace MQ.MultiAgent
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Mirror;
    using UnityEngine.UI;
    using Tobii.Research.Unity;

    public class PlayerProperties : NetworkBehaviour
    {

        [SyncVar]
        public int playerID;
        private string playerName;

        [SerializeField]
        GameObject[] playerComponents;

        private Transform myCamera;
        public Transform myFace { get; private set; }
        public Transform myBody { get; private set; }
        public Transform myUpperArm { get; private set; }
        public Transform myLowerArm { get; private set; }
        public Transform myHand { get; private set; }
        public Transform myGazeCue { get; private set; }
        private RectTransform myGazeCueRect;
        private Text myGazeText;
        private bool gazeCueIsRunning = false;
        private int gazeCue;
        private Text myCubeCueText;

        private SyncActors syncActor;

        #region Unity Methods
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            myCamera = this.transform.Find("Camera");
            myCamera.gameObject.GetComponent<Camera>().enabled = true;
            myCamera.gameObject.tag = "MainCamera";
            this.gameObject.tag = "Player";
            SetName(playerID);
            AddPlayerComponents();

            //Name Face, Body and Hand.
            myBody = this.transform.Find("Avatar").Find("mixamorig:Hips").Find("mixamorig:Spine");
            myBody.GetComponent<BoxCollider>().enabled = false;

            myFace = myBody.Find("mixamorig:Spine1").Find("mixamorig:Spine2").Find("mixamorig:Neck").Find("mixamorig:Head");
            myFace.GetComponent<SphereCollider>().enabled = false;

            myUpperArm = myBody.Find("mixamorig:Spine1").Find("mixamorig:Spine2").Find("mixamorig:RightShoulder").Find("mixamorig:RightArm");
            myUpperArm.GetComponent<CapsuleCollider>().enabled = false;

            myLowerArm = myUpperArm.Find("mixamorig:RightForeArm");
            myLowerArm.GetComponent<CapsuleCollider>().enabled = false;

            myHand = myLowerArm.Find("mixamorig:RightHand").Find("mixamorig:RightHandIndex1");
            myHand.GetComponent<BoxCollider>().enabled = false;

            SetObjName(myFace, "Face", playerID);
            SetObjName(myBody, "Body", playerID);
            SetObjName(myUpperArm, "UpperArm", playerID);
            SetObjName(myLowerArm, "LowerArm", playerID);
            SetObjName(myHand, "Hand", playerID);

            myGazeCue = this.transform.Find("GazeCueCanvas");
            myGazeCueRect = myGazeCue.GetComponent<RectTransform>();
            myGazeText = myGazeCue.Find("Text").GetComponent<Text>();

            //Rotate Gaze Object to match orientation of Player
            if (this.name == "Player2") myGazeCueRect.eulerAngles = new Vector3(0, 180, 0);

            syncActor = this.GetComponent<SyncActors>();

        }

        private void Update()
        {
            //Update Variables if not the local player
            if (!isLocalPlayer)
            {
                if (playerName == null)
                {
                    this.gameObject.tag = "OtherPlayer";

                    var myPlayer = GameObject.FindGameObjectWithTag("Player");
                    myPlayer.GetComponent<SyncActors>().partnerPlayer = new OtherPlayer(this.gameObject);
                    ExperimentManager.Instance.InitializeDataHolder();
                    this.GetComponent<SyncActors>().partnerFound = true;

                    SetName(playerID);

                    //Name Face, Body and Hand.
                    myBody = this.transform.Find("Avatar").Find("mixamorig:Hips").Find("mixamorig:Spine");
                    myFace = myBody.Find("mixamorig:Spine1").Find("mixamorig:Spine2").Find("mixamorig:Neck").Find("mixamorig:Head");
                    myUpperArm = myBody.Find("mixamorig:Spine1").Find("mixamorig:Spine2").Find("mixamorig:RightShoulder").Find("mixamorig:RightArm");
                    myLowerArm = myUpperArm.Find("mixamorig:RightForeArm");
                    myHand = myLowerArm.Find("mixamorig:RightHand").Find("mixamorig:RightHandIndex1");

                    SetObjName(myFace, "Face", playerID);
                    SetObjName(myBody, "Body", playerID);
                    SetObjName(myUpperArm, "UpperArm", playerID);
                    SetObjName(myLowerArm, "LowerArm", playerID);
                    SetObjName(myHand, "Hand", playerID);
                }
            }
            if (isLocalPlayer)
            {
                if (syncActor.partnerPlayer != null)
                {
                    myGazeCueRect.position = syncActor.partnerPlayer.Head.position;
                    myGazeCueRect.position += syncActor.partnerPlayer.Head.forward * (0.1f);
                }
            }
        }
        #endregion

        #region Helper Functions
        private void AddPlayerComponents()
        {
            foreach (GameObject obj in playerComponents)
            {
                Instantiate(obj, Vector3.zero, Quaternion.identity);
            }

        }
        private void SetName(int ID)
        {
            playerName = "Player" + ID.ToString();
            this.transform.name = playerName;
        }

        private void SetObjName(Transform obj, string objName, int ID)
        {
            string name = playerName + objName;
            obj.name = name;
        }
        #endregion

        #region Task Methods (Individual, not Synced)
        public void RunGazeSequence(TrialInfo trial)
        {
            if (this.name == "Player1") return;

            if (gazeCueIsRunning)
            {
                StopCoroutine("ShowGazeSequence");
                ResetColors();
                gazeCueIsRunning = false;
            }

            StartCoroutine(ShowGazeSequence(trial));
        }
        public void ShowGazeCue(TrialInfo trial, bool state)
        {
            if (this.name == "Player1") return;

            if (state)
            {
                //Reverse AV_Display to match Player2's perspective
                gazeCue = trial.AV_Display;
                if (gazeCue == 1) gazeCue = 3;
                else if (gazeCue == 3) gazeCue = 1;


                Debug.Log("Predictive Gaze: " + trial.PredictiveGaze);

                if (trial.PredictiveGaze == -1) //Confederate acting as Responder
                {
                    myGazeText.color = Color.green;
                    myGazeText.text = "R";
                }
                else
                {
                    if (trial.PredictiveGaze == 1) myGazeText.color = Color.green;
                    else if (trial.PredictiveGaze == 0) myGazeText.color = Color.red;
                    else Debug.LogError("Error: trial.PredictiveGaze is not what was expected.");

                    if (gazeCue == 1) myGazeText.text = "LEFT";
                    else if (gazeCue == 2) myGazeText.text = "MIDDLE";
                    else if (gazeCue == 3) myGazeText.text = "RIGHT";
                    else Debug.LogError("Error: trial.gazeCue is not what was expected.");
                }
            }
            else
            {
                myGazeText.text = "";
            }

        }
        IEnumerator ShowGazeSequence(TrialInfo trial)
        {
            gazeCueIsRunning = true;

            //Show Cube Gaze Sequence
            int gazeCount = 0;
            int[] gazeIndxs = new int[4] { trial.Gaze_1, trial.Gaze_2, trial.Gaze_3, trial.Gaze_4 };
            GameObject currentObj;
            while (true)
            {
                if (gazeIndxs[gazeCount] == 0) break;

                //Change target object color
                currentObj = ExperimentManager.Instance.MyBlocks[gazeIndxs[gazeCount] - 1];

                ResetColors();

                ChangeColor(currentObj, ExperimentManager.Instance.myYellow);

                //Wait for raycast hit on a block object.
                while (true)
                {
                    if (VRGazeTrail.Instance.LatestHitObject == currentObj.transform) break;
                    yield return null;
                }

                yield return new WaitForSeconds(0.150f);
                gazeCount++;

                //If this was last gaze, change to blue to indicate end.
                if (gazeCount == 4 || gazeIndxs[gazeCount] == 0)
                {
                    currentObj = ExperimentManager.Instance.MyBlocks[gazeIndxs[gazeCount-1] - 1];

                    if (trial.PredictiveGaze == -1) ChangeColor(currentObj, ExperimentManager.Instance.myDarkGreen);
                    else
                    {
                        ChangeColor(currentObj, ExperimentManager.Instance.myBlue);    //Blue if last object.
                        SetCubeCue(currentObj, trial.PredictiveGaze);

                    }

                    break;
                }
                yield return null;

            }
            myGazeText.text = "";
            gazeCueIsRunning = false;
        }
        void SetCubeCue(GameObject cube, int predictive)
        {
            if (this.name == "Player1") return;

            myCubeCueText = cube.transform.Find("CanvasB").GetComponentInChildren<Text>();

            if (predictive == 1) myCubeCueText.text = "YES";
            else
            {
                if (gazeCue == 1) myCubeCueText.text = "LEFT";
                else if (gazeCue == 2) myCubeCueText.text = "MIDDLE";
                else if (gazeCue == 3) myCubeCueText.text = "RIGHT";
            }
        }
        void ChangeColor(GameObject obj, Material NewColor)
        {
            obj.GetComponent<Renderer>().material = NewColor;
        }
        private void ResetColors()
        {
            if (this.name == "Player1") return;

            foreach (GameObject block in ExperimentManager.Instance.MyBlocks)
            {
                ChangeColor(block, ExperimentManager.Instance.myUnselected);
            }

            if (myCubeCueText != null) myCubeCueText.text = "";
        }
        public void ResetGazeSeq()
        {
            if (this.name == "Player1") return;

            if (gazeCueIsRunning)
            {
                StopCoroutine("ShowGazeSequence");
                gazeCueIsRunning = false;
            }
            ResetColors();

        }
        #endregion
    }
}
