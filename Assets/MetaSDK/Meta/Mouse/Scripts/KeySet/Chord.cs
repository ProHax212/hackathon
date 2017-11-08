using UnityEngine;
using System;

namespace Meta
{
    /// <summary>
    /// A set of keyboard keys that should be pressed simultaneously to perform some action.
    /// </summary>
    /// <example>Control-C: PrimaryKeys: KeyCode.C, ModifierKeys: KeyCode.LeftControl, KeyCode.RightControl</example>
    [Serializable]
    public class Chord
    {
        /// <summary>
        /// All keys in this set must be pressed
        /// </summary>
        [SerializeField]
        private KeyCode[] _primaryKeys;
        /// <summary>
        /// At least one modifier key must be pressed
        /// </summary>
        [SerializeField]
        private Subchord[] _modifierKeys;

        private IKeyboardWrapper _keyboardWrapper;

        /// <summary>
        /// All keys in this set must be pressed
        /// </summary>
        public KeyCode[] PrimaryKeys
        {
            get { return _primaryKeys; }
        }

        /// <summary>
        /// At least one modifier key must be pressed
        /// </summary>
        public Subchord[] ModifierKeys
        {
            get { return _modifierKeys; }
        }

        private IKeyboardWrapper KeyboardWrapper
        {
            get { return _keyboardWrapper ?? (_keyboardWrapper = GameObject.FindObjectOfType<MetaContextBridge>().CurrentContext.Get<IKeyboardWrapper>()); }
        }

        public Chord()
        {
        }

        public Chord(KeyCode[] primaryKeys, Subchord[] modifierKeys) : this()
        {
            _primaryKeys = primaryKeys;
            _modifierKeys = modifierKeys;

            if (_primaryKeys == null || _primaryKeys.Length == 0)
            {
                Debug.LogError("No Keys set in the Chord!");
            }
        }

        /// <summary>
        /// Are the primary keys and any modifier keys pressed?
        /// </summary>
        /// <returns>True if all primary keys and at least one modifier key (if any are defined) are pressed</returns>
        public bool IsPressed()
        {
            bool modifierPressed = _modifierKeys.Length == 0;

            for (int i = 0; i < _modifierKeys.Length; i++)
            {
                if (_modifierKeys[i].IsPressed())
                {
                    modifierPressed = true;
                    break;
                }
            }

            if (modifierPressed)
            {
                for (int i = 0; i < _primaryKeys.Length; i++)
                {
                    if (!KeyboardWrapper.GetKey(_primaryKeys[i]))
                    {
                        modifierPressed = false;
                    }
                }
            }

            return modifierPressed;
        }

        /// <summary>
        /// If the keys are correctly down, returns true when a primary key is released.
        /// </summary>
        /// <returns></returns>
        public bool GetUp()
        {
            return CheckKeys(KeyboardWrapper.GetKeyUp);
        }

        /// <summary>
        /// Checks if the last primary key not down has been pressed down. At least one modifier key, if any are defined, must be down.
        /// </summary>
        /// <returns></returns>
        public bool GetDown()
        {
            return CheckKeys(KeyboardWrapper.GetKeyDown);
        }

        /// <summary>
        /// Check that all keys, but one are down. Returns true if a check function performed on the key that is not down returns true.
        /// </summary>
        /// <param name="check">Check to perform on a key</param>
        /// <returns>True if all keys but one are down and the check function applied to the remaining key returns true.</returns>
        private bool CheckKeys(Func<KeyCode, bool> check)
        {
            bool modifierPressed = _modifierKeys.Length == 0;

            for (int i = 0; i < _modifierKeys.Length; i++)
            {
                if (_modifierKeys[i].IsPressed())
                {
                    modifierPressed = true;
                    break;
                }
            }

            if (modifierPressed)
            {
                bool checksPassed = false;

                for (int i = 0; i < _primaryKeys.Length; i++)
                {
                    if (check(_primaryKeys[i]))
                    {
                        checksPassed = true;
                    }
                    else if (!KeyboardWrapper.GetKey(_primaryKeys[i]))
                    {
                        return false;
                    }
                }

                return checksPassed;
            }

            return false;
        }
    }
}