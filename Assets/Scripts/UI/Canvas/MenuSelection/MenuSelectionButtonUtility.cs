using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Canvas.MenuSelection
{
    public static class MenuSelectionButtonUtility
    {
        public static void BuildButtons(List<Button> buttons, int needed)
        {
            if (buttons == null || buttons.Count == 0 || needed <= buttons.Count)
                return;

            var template = buttons[0];
            var templateRect = template.transform as RectTransform;
            if (templateRect == null)
                return;

            var parent = template.transform.parent;
            var step = -130f;

            if (buttons.Count > 1)
            {
                var secondRect = buttons[1].transform as RectTransform;
                if (secondRect != null)
                    step = secondRect.anchoredPosition.y - templateRect.anchoredPosition.y;
            }

            for (var index = buttons.Count; index < needed; index++)
            {
                var instance = Object.Instantiate(template, parent);
                instance.name = $"{template.name}_{index + 1}";

                var instanceRect = instance.transform as RectTransform;
                if (instanceRect != null)
                {
                    instanceRect.anchoredPosition = new Vector2(
                        templateRect.anchoredPosition.x,
                        templateRect.anchoredPosition.y + step * index);
                }

                buttons.Add(instance);
            }
        }
    }
}
