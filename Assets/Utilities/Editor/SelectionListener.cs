using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CustomUtilities.EditorNamespace
{
    public class SelectionListener<T>
        where T : MonoBehaviour
    {
        private bool needUpdateHash;
        private T[] cachedActiveNodes;
        private T[] cachedDisableNodes;
        private T[] cachedActivatedNodes;

        public SelectionListener()
        {
            cachedActiveNodes = Array.Empty<T>();
            cachedDisableNodes = Array.Empty<T>();
            cachedActivatedNodes = Array.Empty<T>();

            Selection.selectionChanged += OnSelectionChanged;
        }

        ~SelectionListener()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }


        private void OnSelectionChanged() => needUpdateHash = true;

        public IEnumerable<T> GetActive()
        {
            UpdateCache();
            return cachedActiveNodes;
        }

        public IEnumerable<T> GetDisabled()
        {
            UpdateCache();
            return cachedDisableNodes;
        }

        public IEnumerable<T> GetActivated()
        {
            UpdateCache();
            return cachedActivatedNodes;
        }

        private void UpdateCache()
        {
            if (!needUpdateHash)
            {
                return;
            }

            var oldHashedNodes = cachedActiveNodes;

            cachedActiveNodes = Selection.gameObjects
                .Where(x => x.scene.name != null && x.scene.name != x.name)
                .Select(o => o.GetComponent<T>())
                .Where(o => o != null)
                .ToArray();

            cachedDisableNodes = oldHashedNodes
                .Except(cachedActiveNodes)
                .Where(x => x != null)
                .ToArray();

            cachedActivatedNodes = cachedActiveNodes
                .Except(oldHashedNodes)
                .Where(x => x != null)
                .ToArray();

            needUpdateHash = false;
        }
    }
}