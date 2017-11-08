using UnityEngine;
using System;

namespace Meta
{
    /// <summary>
    /// A set of keys where only one key needs to be pressed
    /// </summary>
    [Serializable]
    public class Subchord
    {
        [SerializeField]
        private KeyCode[] _keys;

        private IKeyboardWrapper _keyboardWrapper;

        /// <summary>
        /// The set of keys to check. If any of the keys meet the conditions of an event, the event check function
        /// returns true.
        /// </summary>
        public KeyCode[] Keys
        {
            get { return _keys; }
        }

        private IKeyboardWrapper KeyboardWrapper
        {
            get { return _keyboardWrapper ?? (_keyboardWrapper = GameObject.FindObjectOfType<MetaContextBridge>().CurrentContext.Get<IKeyboardWrapper>()); }
        }

        /// <summary>
        /// Check if one of the keys is pressed
        /// </summary>
        public bool IsPressed()
        {
            return CheckKeys(KeyboardWrapper.GetKey);
        }

        /// <summary>
        /// Check if one of the keys had a key up
        /// </summary>
        /// <returns></returns>
        public bool GetUp()
        {
            return CheckKeys(KeyboardWrapper.GetKeyUp);
        }

        /// <summary>
        /// Check if one of the keys had a key down
        /// </summary>
        public bool GetDown()
        {
            return CheckKeys(KeyboardWrapper.GetKeyDown);
        }

        /// <summary>
        /// Check that one of the keys passes the check function
        /// </summary>
        /// <param name="check">Check to perform on a key</param>
        private bool CheckKeys(Func<KeyCode, bool> check)
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                if (check(_keys[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
