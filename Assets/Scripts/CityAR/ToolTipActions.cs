using System.Text.RegularExpressions;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace CityAR
{
    public class ToolTipActions : MonoBehaviour, IMixedRealityTouchHandler
    {
        private GameObject _lastTouched;
        private int _currentState = 0;

        private static string GetNextMetric(string currentMetric)
        {
            var output = currentMetric switch
            {
                "LoC" => "#Methods",
                "#Methods" => "#AbsClasses",
                "#AbsClasses" => "#Interfaces",
                "#Interfaces" => "LoC",
                _ => "LoC"
            };
            return output;
        }

        // Toggles a single tooltip on and off
        private static void ShowToolTip(string toolTipName)
        {
            var toolTipInstance = getToolTip(toolTipName);
            if (toolTipInstance == null) return;
            var toolTipConnector = toolTipInstance.GetComponent<ToolTipConnector>();
            var toolTip = toolTipInstance.GetComponent<ToolTip>();
            var simpleLineDataProvider = toolTipInstance.GetComponent<SimpleLineDataProvider>();
            if (toolTip.enabled) return;
            toolTip.enabled = true;
            toolTipConnector.enabled = true;
            simpleLineDataProvider.enabled = true;
        }
        
        private static GameObject getToolTip(string toolTipName)
        {
            var allToolTips = FindObjectsOfType<ToolTip>();
            GameObject toolTipInstance = null;
            foreach (var singleToolTip in allToolTips)
            {
                if (singleToolTip.name.StartsWith(toolTipName))
                {
                    toolTipInstance = singleToolTip.gameObject;
                }
            }
            return toolTipInstance;
        }

        private void ToggleText(string toolTipName)
        {
            var toolTipInstance = getToolTip(toolTipName);
            if (toolTipInstance == null) return;
            var match = Regex.Match(toolTipInstance.name, @".*\(([\d;]*)\) Tooltip").Groups[1].Value;
            var values = match.Split(';');
            var metricName = new[] {"LoC", "#Methds", "#AbsCls", "#Intfs"};
            var toolTip = toolTipInstance.GetComponent<ToolTip>();
            toolTip.ToolTipText = $"{metricName[_currentState]}: {values[_currentState]}";
        }

        // Shows a single tooltip for a given building
        private void ShowTooltip()
        {
            var parent = gameObject.transform.parent;
            if (_lastTouched == parent.gameObject)
            {
                _currentState++;
                if (_currentState > 3) _currentState = 0;
                ToggleText(parent.name);
                Debug.Log("Switched Metric");
            }
            ShowToolTip(parent.name);
            _lastTouched = parent.gameObject;
        }

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            ShowTooltip();
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            ShowTooltip();
        }

        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
        }
    }
}