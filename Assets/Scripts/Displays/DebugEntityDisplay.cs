using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;

public class DebugEntityDisplay : EntityDisplayComponent
{
    [SerializeField]
    private bool isEnabled;
    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private TextMeshProUGUI text;

    public override void OnInit(Entity entity)
    {
        canvas.SetActive(isEnabled);
    }

    public override void OnFixedUpdate(Entity entity)
    {
        if(!isEnabled) return;
        StringBuilder sb = new StringBuilder();
        FieldInfo[] fields = entity.GetType().GetFields(BindingFlags.Public | 
                                              BindingFlags.NonPublic | 
                                              BindingFlags.Instance);
        for (int i = 0; i < fields.Length; i++)
        {
           var f = fields[i];
           var val = f.GetValue(entity);
           sb.AppendLine($"{f.Name}: {val?.ToString()}");

        }

        text.text = sb.ToString();
    }
}
