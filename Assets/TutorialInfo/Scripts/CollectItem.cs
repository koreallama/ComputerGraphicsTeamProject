using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TMPro.Examples;

public class CollectItem : MonoBehaviour
{
    public float pickupRange = 2f;
    public float sphereRadius = 0.3f;
    public float pickupAngle = 30f;
    private bool isSwapping = false;
    private int? firstSlot = null;
    private int selectedSlot = -1; // 선택된 슬롯 인덱스 (-1이면 없음)


    public LayerMask itemLayer;
    public string itemLayerName = "Item";

    public TextMeshProUGUI pickupText;
    public InventoryUIController inventoryUI;  // UI 연동용

    private GameObject currentItem;
    private Inventory inventory = new Inventory();

    void Start()
    {
        itemLayer = LayerMask.GetMask(itemLayerName);
    }

    void Update()
    {
        ShowPickupPrompt();

        if (Input.GetKeyDown(KeyCode.E)) TryPickupItem();
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) selectedSlot = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) selectedSlot = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) selectedSlot = 2;

        if (Input.GetMouseButtonDown(0)) UseSelectedItem();

        if (Input.GetKeyDown(KeyCode.R))
        {
            isSwapping = true;
            firstSlot = null;
            Debug.Log("[swap 모드] 시작 : 바꿀 첫 번째 슬롯 번호를 누르세요 (1~3)");
        }

        if (isSwapping)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) HandleSwapInput(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) HandleSwapInput(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) HandleSwapInput(2);
        }
    }

    void UseSelectedItem()
    {
        if (selectedSlot < 0 || selectedSlot >= inventory.items.Count)
        {
            Debug.LogWarning("선택된 슬롯이 비어 있거나 범위 초과");
            return;
        }

        GameObject item = inventory.GetItem(selectedSlot);
        if (item == null)
        {
            Debug.LogWarning("슬롯에 저장된 아이템이 없습니다.");
            return;
        }

        Vector3 spawnPos = transform.position + transform.forward * 1.5f;
        spawnPos.y = 1.0f;

        GameObject clone = Instantiate(item);
        clone.name = item.name;
        clone.tag = "Item";
        clone.layer = LayerMask.NameToLayer("Item");
        clone.SetActive(true);
        clone.transform.position = spawnPos;
        clone.transform.rotation = Quaternion.identity;

        Debug.Log($"[사용] {selectedSlot + 1}번 슬롯 아이템 '{item.name}' 소환됨");

        inventory.RemoveItem(selectedSlot);
        inventoryUI.ClearSlotIcon(selectedSlot);
        selectedSlot = -1;
    }

    void HandleSwapInput(int slot)
    {
        if (firstSlot == null)
        {
            firstSlot = slot;
            Debug.Log($"[교환 모드] 첫 번째 슬롯: {slot + 1}번 선택됨. 바꿀 두 번째 슬롯 번호를 누르세요.");
        }
        else
        {
            int secondSlot = slot;
            int first = firstSlot.Value;
            firstSlot = null;
            isSwapping = false;

            inventory.SwapItems(first, secondSlot);
            inventoryUI.SwapIcons(first, secondSlot);
            Debug.Log($"[교환 완료] {first + 1}번 ↔ {secondSlot + 1}번 슬롯의 아이템이 교환되었습니다.");
        }
    }

    void UseItem(int index)
    {
        GameObject item = inventory.GetItem(index);
        if (item != null)
        {
            Debug.Log($"[사용] {index + 1}번 슬롯의 아이템 '{item.name}'을 사용합니다.");
            // TODO: 실제 사용 로직 여기에 추가
        }
        // else
        // {
        //     Debug.Log($"[사용 실패] {index + 1}번 슬롯이 비어있습니다.");
        // }
    }

    void TryPickupItem()
    {
        Transform player = this.transform;
        Vector3 eyePosition = player.position + Vector3.up * 1.5f;
        Vector3 viewDirection = player.forward;

        Ray ray = new Ray(eyePosition, viewDirection);
        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.green, 1.0f);

        RaycastHit hit;
        if (Physics.SphereCast(ray, sphereRadius, out hit, pickupRange, itemLayer))
        {
            GameObject target = hit.collider.gameObject;

            Vector3 directionToItem = (target.transform.position - eyePosition).normalized;
            float angleToItem = Vector3.Angle(viewDirection, directionToItem);

            if (angleToItem <= pickupAngle)
            {
                float itemDistance = Vector3.Distance(eyePosition, target.transform.position);
                float t = Mathf.Clamp01(itemDistance / pickupRange);
                float scaleFactor = Mathf.Lerp(0.5f, 2.0f, t);
                target.transform.localScale *= scaleFactor;

                GameObject clone = Instantiate(target);
                clone.SetActive(false);
                clone.name = target.name;
                clone.tag = "Item";
                clone.layer = LayerMask.NameToLayer("Item");

                ItemData data = target.GetComponent<ItemData>();
                if (data != null && data.icon != null)
                {
                    int index = inventory.AddItem(clone);

                    if (index == -1)
                    {
                        Destroy(clone); // 메모리 정리
                        ShowToast("아이템이 꽉 찼습니다!");
                        return;
                    }

                    inventoryUI.AddItemToUI(data.icon);
                    selectedSlot = index; // 자동 선택
                    Debug.Log($"[인벤토리] {index + 1}번 슬롯에 {target.name} 저장됨");
                }

                Destroy(target);
                currentItem = null;
                pickupText.gameObject.SetActive(false);
            }
        }
    }


    void ShowPickupPrompt()
    {
        Transform player = this.transform;
        Ray ray = new Ray(player.position + Vector3.up * 1.2f, player.forward);
        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.red);

        RaycastHit hit;
        if (Physics.SphereCast(ray, sphereRadius, out hit, pickupRange, itemLayer))
        {
            // Debug.Log("맞은 물체: " + hit.collider.name);

            if (hit.collider.CompareTag("Item"))
            {
                currentItem = hit.collider.gameObject;
                pickupText.text = $"Press 'E' to pick up \"{currentItem.name}\"";
                pickupText.gameObject.SetActive(true);
                return;
            }
        }

        pickupText.gameObject.SetActive(false);
        currentItem = null;
    }

    void ShowToast(string message)
    {
        pickupText.text = message;
        pickupText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideToast)); // 중복 방지
        Invoke(nameof(HideToast), 1.5f); // 1.5초 뒤 숨기기
    }

    void HideToast()
    {
        pickupText.gameObject.SetActive(false);
    }

}
