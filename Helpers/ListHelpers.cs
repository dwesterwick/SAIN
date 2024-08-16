using Audio.Data;
using EFT;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace SAIN.Helpers
{
    public class ListCache<T>
    {
        public event Action<T> OnAddItemToCache;

        public event Action<T> OnRemoveItemFromCache;

        public readonly string Name;

        public ListCache(string name)
        {
            Name = name;
            GameWorld.OnDispose += clearCache;
        }

        public void Dispose()
        {
            GameWorld.OnDispose -= clearCache;
        }

        private void clearCache()
        {
            Cache.Clear();
        }

        public readonly List<T> Cache = new List<T>();

        public void HandleCache(List<T> list, int targetCount)
        {
            AddOrRemove(list, targetCount - list.Count);
        }

        public void AddOrRemove(List<T> list, int addOrSubtract)
        {
            if (addOrSubtract != 0) {
                if (addOrSubtract > 0) {
                    fill(list, addOrSubtract);
                }
                else {
                    trim(list, addOrSubtract);
                }
                Logger.LogDebug($"[{Cache.Count}] items in cache [{Name}]");
            }
        }

        public void ReturnAllToCache(List<T> list)
        {
            int count = list.Count;
            if (count == 0) {
                return;
            }
            Cache.AddRange(list);
            list.Clear();
            Logger.LogDebug($"returned [{count}] items in cache [{Name}] cacheCount: [{Cache.Count}]");
        }

        private void trim(List<T> list, int trimAmount)
        {
            if (trimAmount < 0) {
                trimAmount = 0;
            }
            int listCount = list.Count;
            for (int i = listCount - 1; i >= 0; i--) {
                T item = list[i];

                Cache.Add(item);
                list.RemoveAt(i);
                OnAddItemToCache?.Invoke(item);

                // fucking loops breaking my brain
                trimAmount++;
                if (trimAmount == 0) {
                    break;
                }
            }
            Logger.LogDebug($"trimmed list of [{trimAmount}] items. old total: [{listCount}] new total: [{list.Count}]",
                "trim(List<T> list, int countToRemove)");
        }

        private void fill(List<T> list, int countToAdd)
        {
            int targetCount = list.Count + countToAdd;
            if (Cache.Count < countToAdd) {
                createCacheItems(countToAdd);
            }

            int cacheCount = Cache.Count;
            for (int i = cacheCount - 1; i >= 0; i--) {
                T item = Cache[i];
                list.Add(item);
                OnRemoveItemFromCache?.Invoke(item);
                Cache.RemoveAt(i);

                countToAdd--;
                if (countToAdd == 0) {
                    break;
                }
            }

            if (list.Count != targetCount) {
                Logger.LogError("list.Count != targetCount");
            }
        }

        private void createCacheItems(int countToAdd)
        {
            int cacheCount = Cache.Count;
            for (int i = 0; i < countToAdd; i++) {
                Cache.Add((T)Activator.CreateInstance(typeof(T)));
            }
            Logger.LogDebug($"filled cache with [{countToAdd}] items. old total: [{cacheCount}] new total: [{Cache.Count}]",
                "fillCache(int countToAdd)");
        }
    }

    internal static class ListCacheHelpers
    {
        private static void MoveIndexToList<T>(List<T> origin, List<T> target, int index)
        {
            target.Add(origin[index]);
            origin.RemoveAt(index);
        }
    }

    internal static class ListHelpers
    {
        public static void MoveIndexToList<T>(List<T> origin, List<T> target, int index)
        {
            target.Add(origin[index]);
            origin.RemoveAt(index);
        }

        public static bool ClearCache<T, V>(Dictionary<T, V> list)
        {
            if (list != null && list.Count > 0) {
                list.Clear();
                return true;
            }
            return false;
        }
    }
}