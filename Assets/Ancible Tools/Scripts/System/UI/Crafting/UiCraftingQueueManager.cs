using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Items.Crafting;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Crafting
{
    public class UiCraftingQueueManager : MonoBehaviour
    {
        private const string FILTER = "UI_CRAFTING_QUEUE_MANAGER";

        [SerializeField] private UiQueuedCraftController _queuedCraftTemplate;
        [SerializeField] private HorizontalLayoutGroup _grid;

        private GameObject _owner;
        private int _maxSlots = 0;
        private UiQueuedCraftController[] _controllers = new UiQueuedCraftController[0];
        private string _filter = string.Empty;

        public void Setup(GameObject owner)
        {
            _owner = owner;
            _filter = $"{FILTER}{_owner.GetInstanceID()}";
            RefreshOwner();
            SubscribeToMessages();
        }

        private void RefreshOwner()
        {
            var queryCraftingQueueMsg = MessageFactory.GenerateQueryCraftingQueueMsg();
            queryCraftingQueueMsg.DoAfter = RefreshQueue;
            gameObject.SendMessageTo(queryCraftingQueueMsg, _owner);
            MessageFactory.CacheMessage(queryCraftingQueueMsg);
        }

        private void RefreshQueue(QueuedCraft[] queue, int maxSlots)
        {
            if (_controllers.Length < maxSlots)
            {
                var controllers = _controllers.ToList();
                var currentCount = controllers.Count;
                for (var i = currentCount; i < maxSlots; i++)
                {
                    var controller = Instantiate(_queuedCraftTemplate, _grid.transform);
                    controller.SetIndex(i, maxSlots - 1);
                    controller.SetOwner(_owner);
                }
            }

            for (var i = 0; i < maxSlots; i++)
            {
                var controller = _controllers[i];
                if (queue.Length >= i)
                {
                    if (controller.Craft == null || controller.Craft != queue[i])
                    {
                        if (controller.Craft == null)
                        {
                            controller.Setup(queue[i]);
                        }
                        else
                        {
                            controller.RefreshCraft();
                        }
                    }
                }
                else if (controller.Craft != null)
                {
                    controller.Clear();
                }
            }
        }

        private void SubscribeToMessages()
        {
            _owner.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, _filter);
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshOwner();
        }

        public void Destroy()
        {
            _owner.UnsubscribeFromAllMessagesWithFilter(_filter);
            gameObject.UnsubscribeFromAllMessages();
        }
        
    }
}