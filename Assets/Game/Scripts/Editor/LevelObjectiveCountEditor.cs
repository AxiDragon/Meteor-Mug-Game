using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelObjectiveCount))]
public class LevelObjectiveCountEditor : Editor
{
    private static int chickCount = 0;
    private static bool roundUp = true;
    [Range(0, 1)] private static float controlNeededForWin = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Calculate Chick Count"))
        {
            chickCount = FindObjectsOfType<ChickController>().Length;
            LevelObjectiveCount levelObjectiveCount = (LevelObjectiveCount)target;
            levelObjectiveCount.objectiveCount[0] = chickCount;
        }

        GUI.enabled = false;
        EditorGUILayout.IntField("Chick Count", chickCount);
        GUI.enabled = true;

        controlNeededForWin =
            EditorGUILayout.Slider(
                new GUIContent("Win Percentage",
                    "Extra percentage of chicks needed above (available chicks / players)"),
                controlNeededForWin, 0f, 1f);

        roundUp = EditorGUILayout.Toggle("Round up win chick count value", roundUp);
        if (GUILayout.Button("Calculate Win Chick Count"))
        {
            chickCount = FindObjectsOfType<ChickController>().Length;
            LevelObjectiveCount levelObjectiveCount = (LevelObjectiveCount)target;

            for (int i = 0; i < levelObjectiveCount.objectiveCount.Length; i++)
            {
                float c = chickCount / ((float)i + 1) * (1 + controlNeededForWin);
                levelObjectiveCount.objectiveCount[i] = roundUp ? Mathf.CeilToInt(c) : Mathf.FloorToInt(c);
            }
            
            levelObjectiveCount.objectiveCount[0] = chickCount;
            EditorUtility.SetDirty(this);
        }
    }
}