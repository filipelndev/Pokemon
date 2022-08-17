using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text nameText;
    [SerializeField] Text descriptionText;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [SerializeField] PartyScreen partyScreen;

    InventoryUIState state;
    
    int selectedItem = 0;

    const int itensInViewport = 4;

    Inventory inventory;
    List<ItemSlotUI> slotUIList;
    RectTransform itemListRect;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    public void Start()
    {
        UpdateItemList();
    }

    void UpdateItemList()
    {
        //limpa todos os itens existentes
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();

        foreach(var itemSlot in inventory.Slots)
        {
           var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
           slotUIObj.SetData(itemSlot);

           slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack)
    {

        if(state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;

            if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++selectedItem;
            }
            else if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                --selectedItem;
            }

            selectedItem = Mathf.Clamp(selectedItem, 0 , inventory.Slots.Count-1);

            if(prevSelection != selectedItem)
                UpdateItemSelection();

            if(Input.GetKeyDown(KeyCode.X))
            {
                onBack?.Invoke();
            }
            else if(Input.GetKeyDown(KeyCode.Z))
            {
                OpenPartyScreen();
            }
        }
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {

            };
            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };
            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }
    }
    void UpdateItemSelection() 
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if(i == selectedItem)
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            else
                slotUIList[i].NameText.color = Color.black;

        }

        var item = inventory.Slots[selectedItem];
        itemIcon.sprite = item.Item.Icon;
        descriptionText.text = item.Item.Description;
        nameText.text = item.Item.Name;

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if(slotUIList.Count <= 9) return;
        

        float scrollPos = Mathf.Clamp(selectedItem - itensInViewport/2, 0, selectedItem) * (slotUIList[0].height - 26);
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itensInViewport/2;
        upArrow.gameObject.SetActive(showUpArrow);
        bool showDownArrow = selectedItem + itensInViewport/2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }
    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }
    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.gameObject.SetActive(false);
    }
}
