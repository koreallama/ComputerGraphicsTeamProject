using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public List<GameObject> items = new List<GameObject>();
    public int maxSlots = 3;

    public int AddItem(GameObject item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                return i;
            }
        }

        if (items.Count < maxSlots)
        {
            items.Add(item);
            return items.Count - 1;
        }

        return -1; // 슬롯이 가득 찼음
    }

    public void SwapItems(int i, int j)
    {
        if (i < 0 || j < 0 || i >= items.Count || j >= items.Count) return;

        GameObject temp = items[i];
        items[i] = items[j];
        items[j] = temp;
    }

    public GameObject GetItem(int index)
    {
        if (index >= 0 && index < items.Count)
            return items[index];
        return null;
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < items.Count)
            items[index] = null;
    }

}
