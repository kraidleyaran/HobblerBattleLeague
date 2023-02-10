using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.WorldNodes
{
    public class WorldNodeManager : MonoBehaviour
    {
        private static WorldNodeManager _instance = null;

        private Dictionary<WorldNodeType, List<RegisteredWorldNode>> _nodes = new Dictionary<WorldNodeType, List<RegisteredWorldNode>>();
        private Dictionary<WorldItem, List<RegisteredWorldNode>> _resourceNodes = new Dictionary<WorldItem, List<RegisteredWorldNode>>();
        private Dictionary<WorldSkill,List<RegisteredWorldNode>> _craftingNodes = new Dictionary<WorldSkill, List<RegisteredWorldNode>>();
        private List<RegisteredWorldNode> _allNodes = new List<RegisteredWorldNode>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            SubscribeToMessages();
        }

        public static RegisteredWorldNode RegisterNode(GameObject unit, MapTile tile, WorldNodeType type)
        {
            var node = _instance._allNodes.FirstOrDefault(n => n.Unit == unit);
            if (node == null)
            {
                node = new RegisteredWorldNode(unit, tile, type);
                _instance._allNodes.Add(node);
            }
            if (tile == null)
            {
                tile = WorldController.Pathing.GetMapTileByWorldPosition(unit.transform.position.ToVector2().ToPixelPerfect());
            }

            if (tile != null)
            {
                if (!_instance._nodes.ContainsKey(type))
                {
                    _instance._nodes.Add(type, new List<RegisteredWorldNode>());
                }

                var existing = _instance._nodes[type].FirstOrDefault(n => n.Unit == unit);
                if (existing != null)
                {
                    return existing;
                }

                _instance._nodes[type].Add(node);
                return node;

            }

            return null;
        }

        public static RegisteredWorldNode RegisterResourceNode(GameObject unit, MapTile tile, WorldItem[] items)
        {
            var node = _instance._allNodes.FirstOrDefault(n => n.Unit == unit);
            if (node == null)
            {
                node = new RegisteredWorldNode(unit, tile, WorldNodeType.Resource);
                _instance._allNodes.Add(node);
            }
            for (var i = 0; i < items.Length; i++)
            {
                if (!_instance._resourceNodes.ContainsKey(items[i]))
                {
                    _instance._resourceNodes.Add(items[i], new List<RegisteredWorldNode>());
                }
                _instance._resourceNodes[items[i]].Add(node);
            }

            return node;
        }

        public static RegisteredWorldNode RegisterCraftingNode(GameObject unit, MapTile tile, WorldSkill skill)
        {
            var node = _instance._allNodes.FirstOrDefault(n => n.Unit == unit);
            if (node == null)
            {
                node = new RegisteredWorldNode(unit, tile, WorldNodeType.Crafting);
                _instance._allNodes.Add(node);
            }

            if (!_instance._craftingNodes.TryGetValue(skill, out var craftingNodes))
            {
                craftingNodes = new List<RegisteredWorldNode>();
                _instance._craftingNodes.Add(skill, craftingNodes);
            }

            if (!craftingNodes.Contains(node))
            {
                craftingNodes.Add(node);
            }

            return node;
        }

        public static void UnregisterNode(GameObject unit, WorldNodeType type)
        {
            var existingNode = _instance._allNodes.FirstOrDefault(n => n.Unit == unit);
            if (existingNode != null)
            {
                switch (type)
                {
                    case WorldNodeType.Food:
                    case WorldNodeType.Bed:
                    case WorldNodeType.Book:
                    case WorldNodeType.Activity:
                        if (_instance._nodes.TryGetValue(type, out var wellbeingNodes))
                        {
                            wellbeingNodes.Remove(existingNode);
                            if (wellbeingNodes.Count <= 0)
                            {
                                _instance._nodes.Remove(type);
                            }
                        }
                        break;
                    case WorldNodeType.Resource:
                        var pairs = _instance._resourceNodes.Where(kv => kv.Value.Contains(existingNode)).ToArray();
                        for (var i = 0; i < pairs.Length; i++)
                        {
                            pairs[i].Value.Remove(existingNode);
                            if (pairs[i].Value.Count <= 0)
                            {
                                _instance._resourceNodes.Remove(pairs[i].Key);
                            }
                        }
                        break;
                    case WorldNodeType.Crafting:
                        var craftingPairs = _instance._craftingNodes.Where(kv => kv.Value.Contains(existingNode)).ToArray();
                        foreach (var pair in craftingPairs)
                        {
                            pair.Value.Remove(existingNode);
                            if (pair.Value.Count <= 0)
                            {
                                _instance._craftingNodes.Remove(pair.Key);
                            }
                        }
                        break;
                }
                if (_instance._nodes.TryGetValue(type, out var nodes))
                {
                    var node = nodes.FirstOrDefault(n => n.Unit == unit);
                    if (node != null)
                    {
                        nodes.Remove(node);
                        node.Destroy();
                    }
                }
            }
            
        }

        public static RegisteredWorldNode GetClosestNodeByType(MapTile currentTile, WorldNodeType type)
        {
            if (_instance._nodes.TryGetValue(type, out var nodes))
            {
                if (nodes.Count > 0)
                {
                    return nodes.Count > 1 ? nodes.OrderBy(n => (n.Tile.World - currentTile.World).sqrMagnitude).FirstOrDefault() : nodes[0];
                }

                return null;
            }

            return null;
        }

        public static RegisteredWorldNode GetClosestNodeByItem(MapTile currentTile, WorldItem item)
        {
            if (_instance._resourceNodes.TryGetValue(item, out var nodes))
            {
                if (nodes.Count > 0)
                {
                    return nodes.Count > 1 ? nodes.OrderBy(n => (n.Tile.World - currentTile.World).sqrMagnitude).FirstOrDefault() : nodes[0];
                }

                return null;
            }

            return null;
        }

        public static RegisteredWorldNode GetClosestNodeByCraftingSkill(MapTile currentTile, WorldSkill skill)
        {
            if (_instance._craftingNodes.TryGetValue(skill, out var nodes))
            {
                if (nodes.Count > 0)
                {
                    return nodes.Count > 1 ? nodes.OrderBy(n => (n.Tile.World - currentTile.World).sqrMagnitude).FirstOrDefault() : nodes[0];
                }

                return null;
            }

            return null;
        }

        public static bool IsResourceAvailable(WorldItem item)
        {
            return _instance._resourceNodes.ContainsKey(item);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ClearWorldMessage>(ClearWorld);
        }

        private void ClearWorld(ClearWorldMessage msg)
        {
            _instance._allNodes.Clear();
            _instance._resourceNodes.Clear();
            _instance._nodes.Clear();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}