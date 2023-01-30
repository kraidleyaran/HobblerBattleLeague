using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class DevController : MonoBehaviour
    {
        void Awake()
        {
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private static void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.Save && msg.Current.Save)
            {
                PlayerDataController.SaveData();
            }
            else if (!msg.Previous.Load && msg.Current.Load)
            {
                PlayerDataController.LoadData(PlayerDataController.DefaultPlayerName);
            }
            else if (!msg.Previous.SwitchMode && msg.Current.SwitchMode)
            {
                if (WorldController.State == WorldState.World)
                {
                    WorldController.SetWorldState(WorldState.Adventure);
                }
                else if (WorldController.State == WorldState.Adventure)
                {
                    WorldController.SetWorldState(WorldState.World);
                }
            }
        }
    }
}