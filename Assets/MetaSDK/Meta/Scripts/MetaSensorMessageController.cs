using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Meta
{
    /// <summary>
    /// Controls messages to be displayed on the Sensor Failure UI
    /// </summary>
    internal class MetaSensorMessageController : MonoBehaviour
    {

        [SerializeField]
        private Text _messageText;

        /// <summary>
        /// Changes the message displayed
        /// </summary>
        /// <param name="newMessage">The new message to display.</param>
        internal void ChangeMessage(string newMessage)
        {
            _messageText.text = newMessage;
        }


    }

}