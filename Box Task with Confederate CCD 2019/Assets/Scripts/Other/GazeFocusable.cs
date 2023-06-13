namespace MQ.MultiAgent
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Tobii.G2OM;

    public class GazeFocusable : MonoBehaviour, IGazeFocusable
    {
        //The method of the "IGazeFocusable" interface, which will be called when this object receives or loses focus
        public void GazeFocusChanged(bool hasFocus)
        {
        }
    }
}

