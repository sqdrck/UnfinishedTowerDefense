using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StoreItemUi : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public ResourceUi ItemResource;
    public ResourceUi PriceResource;
    private UnityEngine.Localization.LocalizedString LocStringRef = null;

    public void OnDestroy()
    {
        if (LocStringRef != null)
        {
            LocStringRef.StringChanged -= OnNameChanged;
        }
    }

    public void OnInit(GameState gameState, StoreEntry entry)
    {
        PriceResource.OnInit(gameState, entry.Price.Value);
        ItemResource.OnInit(gameState, entry.Item.Value);
        if (LocStringRef != null)
        {
            LocStringRef.StringChanged -= OnNameChanged;
        }
        if (entry.LocString != null)
        {
            if (!entry.LocString.IsEmpty && NameText != null)
            {
                LocStringRef = entry.LocString;
                LocStringRef.StringChanged += OnNameChanged;
            }
        }
    }

    public void OnNameChanged(string newName)
    {
        NameText.text = newName;
    }
}
