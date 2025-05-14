using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    public Image[] slots = new Image[3];  // UI 슬롯들
    public Sprite emptySlotSprite;        // 빈 칸일 때 기본 회색 배경

    public void AddItemToUI(Sprite icon)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].sprite == emptySlotSprite)
            {
                slots[i].sprite = icon;
                Debug.Log($"[UI] 슬롯 {i}에 아이템 추가됨");
                return;
            }
        }

        Debug.Log("[UI] 인벤토리 꽉 찼음");
    }

    public void SwapIcons(int i, int j)
    {
        if (i < 0 || j < 0 || i >= slots.Length || j >= slots.Length) return;

        Sprite temp = slots[i].sprite;
        slots[i].sprite = slots[j].sprite;
        slots[j].sprite = temp;
    }

    public void ClearUI()
    {
        foreach (var slot in slots)
            slot.sprite = emptySlotSprite;
    }

    public void ClearSlotIcon(int index)
    {
        if (index >= 0 && index < slots.Length)
        {
            slots[index].sprite = emptySlotSprite; // 기본 빈 이미지로 바꿈
        }
    }

}

