using System;
using System.Collections.Generic;
using meta.types;
using UnityEngine;
using HandData = Meta.HandInput.HandData;

namespace Meta
{
    internal class HandsModule : IEventReceiver
    {
        private int _frameId;
        private int _handCount;
        private bool _initialized = false;

        private const int kBuffMaxSize = 1000;
        private byte[] _buffer = new byte[kBuffMaxSize];
        private readonly List<HandData> _activeHands = new List<HandData>();

        private Transform _handsOrigin;
        private HandsProvider _handsProvider;

        public Action<HandData> OnHandDataAppear;
        public Action<HandData> OnHandDataDisappear;


        /// <summary>
        /// Initializes module.
        /// </summary>
        public void Initialize()
        {
            _initialized = true;

            // Find GameObject Depth Occlusion as origin
            var depthOcclusionObject = GameObject.Find("DepthOcclusion");
            if (depthOcclusionObject)
            {
                _handsOrigin = depthOcclusionObject.transform;
            }

            if (_handsProvider != null)
            {
                _handsProvider.Statistics.MarkInitialized();
            }
        }


        void Update()
        {
            if (_initialized)
            {
                CheckNewFrame();
            }
        }

        void IEventReceiver.Init(IEventHandlers eventHandlers)
        {
            eventHandlers.SubscribeOnUpdate(Update);
        }


        /// <summary>
        /// Registers a given HandsProvider Instance.
        /// </summary>
        /// <param name="handsProvider"></param>
        public void RegisterHandsProivder( HandsProvider handsProvider )
        {
            _handsProvider = handsProvider;
            if (_initialized)
            {
                _handsProvider.Statistics.MarkInitialized();
            }
        }


        private void CheckNewFrame()
        {
            FrameHands frameHandsData;
            if (!MetaCocoInterop.GetFrameHandsFlatbufferObject(ref _buffer, out frameHandsData))
            {
                return;
            }

            // -- Skip if old frame id
            bool isSameFrame = (int) frameHandsData.Header.Value.FrameId == _frameId;
            if (isSameFrame) { return; }

            // -- Call on new frame event
            OnNewFrame(frameHandsData);

            // -- Update frame stats
            UpdateFrameStats();
        }

        private void OnNewFrame(FrameHands frameHandsData)
        {
            // -- Mark frame stats
            _frameId = (int)frameHandsData.Header.Value.FrameId;
            _handCount = frameHandsData.HandCount;

            // -- Generate incoming hands list
            List<meta.types.HandData> incoming_hands = new List<meta.types.HandData>();
            for (int i = 0; i < _handCount; i++)
            {
                if (!frameHandsData.Hands(i).HasValue)
                {
                    Debug.LogError("Hand does not have value??!");
                    continue;
                }

                incoming_hands.Add(frameHandsData.Hands(i).Value);
            }


            // -- Iterate over active hands, 
            // -- either:
            // -- 1: match them to new, incoming hands.
            // -- 2: kill them.
            for (int i = _activeHands.Count - 1; i >= 0; i--)
            {
                var active_hand = _activeHands[i];

                bool found_match = false;
                foreach (var incoming_hand in incoming_hands)
                {
                    if (incoming_hand.HandId == active_hand.HandId)
                    {
                        // -- MATCHED!

                        // -- Adopt
                        active_hand.AdoptProperties(incoming_hand, _handsOrigin);

                        // -- Remove From incoming hands list
                        incoming_hands.Remove(incoming_hand);

                        // -- Mark as complete
                        found_match = true;
                        break;
                    }
                }

                // -- No match -- KILL  
                if (!found_match)
                {
                    // -- Invoke on hand exit event
                    if (OnHandDataDisappear != null)
                    { OnHandDataDisappear.Invoke(active_hand); }

                    // -- No match
                    _activeHands.Remove(active_hand);
                }
            }


            // -- Mark remaining hands as new, incoming hands
            foreach (var incoming_hand in incoming_hands)
            {
                // -- Create new HandData 
                var new_hand_data = new HandData(incoming_hand, _handsOrigin);

                // -- Add new hand to active hands list.
                _activeHands.Add(new_hand_data);

                // -- Invoke OnNewHand event
                if (OnHandDataAppear != null)
                { OnHandDataAppear.Invoke(new_hand_data); }
            }
        }

        private void UpdateFrameStats()
        {
            if (_handsProvider != null)
            {
                _handsProvider.Statistics.UpdateFrameData(_frameId, _handCount);
            }
        }

    }
}