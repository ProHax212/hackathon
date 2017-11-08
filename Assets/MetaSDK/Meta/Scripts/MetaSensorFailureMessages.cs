using System.Collections;
using UnityEngine;

namespace Meta
{
    /// <summary>
    /// Creates Sensor Failure messages at predefined intervals.
    /// </summary>
    internal class MetaSensorFailureMessages : IEventReceiver
    {
        private const string SensorFailurePrefabName = "Prefabs/SensorFailureUi";

        private const float IntervalToCheckSensorsSeconds = 1f;

        public void Init(IEventHandlers eventHandlers)
        {
            eventHandlers.SubscribeOnStart(CheckSensors);
        }

        /// <summary>
        /// Creates the Message UI.
        /// </summary>
        /// <returns></returns>
        private MetaSensorMessageController CreateMessageUi()
        {
            GameObject ui = (GameObject) GameObject.Instantiate(Resources.Load(SensorFailurePrefabName));
            return ui.GetComponent<MetaSensorMessageController>();
        }

        private void CheckSensors()
        {
            var manager = GameObject.FindObjectOfType<MetaManager>();
            if (!manager)
            {
                Debug.LogError("Could not get MetaManager");
                return;
            }

            manager.StartCoroutine(CheckSensorsAtIntervals(manager));
        }

        /// <summary>
        /// Check the sensors at the intervals defined.
        /// </summary>
        /// <param name="manager">manager used to obtain Meta's presence in the scene. 
        /// Messages are displayed relative to Meta. Coroutines are started on Meta.
        /// </param>
        /// <returns></returns>
        private IEnumerator CheckSensorsAtIntervals(MetaManager manager)
        {
            if (SensorsInitialized())
            {
                yield break;
            }

            yield return CheckSensorFrequently(10, IntervalToCheckSensorsSeconds, null);

            MetaSensorMessageController controller = CreateMessageUi();
            controller.transform.SetParent(manager.transform);

            if (CheckSensorRecovery(controller))
            {
                yield break;
            }
            
            controller.ChangeMessage("Sensors not yet started.\nPlease wait ...");

            yield return CheckSensorFrequently(20, IntervalToCheckSensorsSeconds, controller.gameObject);

            if (CheckSensorRecovery(controller))
            {
                yield break;
            }

            controller.ChangeMessage("Sensors taking unusually long to start.\nIf this is your first use, this might be normal.\nPlease wait ...");

            yield return CheckSensorFrequently(60, IntervalToCheckSensorsSeconds, controller.gameObject);

            if (CheckSensorRecovery(controller))
            {
                yield break;
            }

            controller.ChangeMessage("Sensors have not started successfully.\nPlease exit, restart your device and\nlaunch the application again.");
        }

        /// <summary>
        /// Checks for sensor recovery.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        private bool CheckSensorRecovery(MetaSensorMessageController controller)
        {
            if (SensorsInitialized())
            {
                if (!DepthSensorWorking())
                {
                    //Not all the sensors have recovered. Show a message indefinitely.
                    controller.ChangeMessage("We've encountered issues starting sensors. Hands might not track.\nExit the application and run Headset Diagnostics.");
                    return true;
                }

                //All sensors recovered, remove the messages.
                GameObject.Destroy(controller.gameObject);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Check the sensors for a number of times at a given interval. 
        /// </summary>
        /// <param name="numberOfChecks">number of times to check</param>
        /// <param name="checkIntervalSeconds">interval to wait in seconds.</param>
        /// <param name="gameObjectToDelete">Delete this gameobject if sensors initialized</param>
        /// <returns></returns>
        private IEnumerator CheckSensorFrequently(int numberOfChecks, float checkIntervalSeconds, GameObject gameObjectToDelete)
        {
            for (int i = 0; i < numberOfChecks; ++i)
            {
                if (SensorsInitialized())
                {
                    if (gameObjectToDelete)
                    {
                        GameObject.Destroy(gameObjectToDelete);
                    }
                    
                    yield break;
                }
                yield return new WaitForSeconds(checkIntervalSeconds);
            }
        }

        /// <summary>
        /// Check if the important sensors have been initialized
        /// </summary>
        /// <returns></returns>
        private bool SensorsInitialized()
        {
            bool rightImuInitialized = false;
            bool leftMonoInitialized = false;
            bool rightMonoInitialized = false;
            bool unused = false;

            MetaSensors.GetSensorConnectionInfo(SensorType.IMU, 0, out unused, out rightImuInitialized);
            MetaSensors.GetSensorConnectionInfo(SensorType.Monochrome, 0, out unused, out rightMonoInitialized);
            MetaSensors.GetSensorConnectionInfo(SensorType.Monochrome, 1, out unused, out leftMonoInitialized);

            return (rightImuInitialized && leftMonoInitialized && rightMonoInitialized);
        }

        /// <summary>
        /// Checks if the depth sensor is working.
        /// </summary>
        /// <returns></returns>
        private bool DepthSensorWorking()
        {
            bool depthSensorConnected;
            bool depthSensorInitialized;
            MetaSensors.GetSensorConnectionInfo(SensorType.IMU, 0, out depthSensorConnected, out depthSensorInitialized);
            return (depthSensorConnected && depthSensorInitialized);
        }
    }

}