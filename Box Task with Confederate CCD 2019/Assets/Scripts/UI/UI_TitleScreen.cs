namespace MQ.MultiAgent
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using Mirror;

    public class UI_TitleScreen : MonoBehaviour
    {
        [SerializeField] Text _participantField;
        [SerializeField] Text _debugMessage;
        [SerializeField] Dropdown _genderSelect;

        [SerializeField] NetworkManagerHUD HUD;

        private string _gender;
        public string Gender
        {
            get { return _gender; }
            set
            {
                _gender = value;
            }
        }

        private void Start()
        {
            HUD.enabled = false;
        }

        private void OnGUI()
        {
            if (!String.IsNullOrEmpty(_participantField.text))
            {
                try
                {
                    if (int.Parse(_participantField.text) >= 0)
                    {
                        TitleScreenData.SubjectNum = int.Parse(_participantField.text);
                        if (_genderSelect.value != 0)
                        {
                            _debugMessage.text = "";
                            HUD.enabled = true;

                            if (_genderSelect.value == 1)
                            {
                                TitleScreenData.Gender = "Female";
                            }
                            else TitleScreenData.Gender = "Male";
                        }
                        else _debugMessage.text = "Select the Avatar's Gender";
                    }
                }
                catch
                {
                    _debugMessage.text = "Participant number must be a non-negative numeric value.";
                }
            }
            else
            {
                HUD.enabled = false;
                _debugMessage.text = "Input participant number.";
            }
        }
    }
}


