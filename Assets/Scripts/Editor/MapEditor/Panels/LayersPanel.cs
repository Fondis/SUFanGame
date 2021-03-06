﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using SUGame.World;
using System.Linq;
using SUGame.Util;

// TODO : Use editorprefs to store settings
namespace SUGame.SUGameEditor.MapEditing.Panels
{
    public class LayersPanel : MapEditorPanel
    {
        public override Rect Area_
        {
            get
            {
                int lineCount = EnumUtil.GetEnumValues<TileLayer>().Count;
                int w = 130;
                int h = Foldout_ ? 18 + (lineCount * 15) + (4 * lineCount) : 22;
                int x = Screen.width - w - 15;
                int y = 15;

                return new Rect(x, y, w, h);
            }
        }

        protected override string FoldoutTitle_
        {
            get
            {
                return "Shape";
            }
        }
        
        static bool[] toggles_ = null;
        //static Map map_;

        const string PREFS_LAYERSFOLDOUT_NAME = "MELayersFoldout";

        public LayersPanel( )
        {
            int layerCount = EnumUtil.GetEnumValues<TileLayer>().Count;

            toggles_ = new bool[layerCount];

        }

        
        protected override void OnPanelGUI( Map map )
        {
            var layers = EnumUtil.GetEnumValues<TileLayer>();
            int layerCount = layers.Count;
            for (int i = 0; i < layerCount; ++i)
            {
                toggles_[i] = map.GetLayerVisible(layers[i]);
            }

            EditorGUI.indentLevel++;
            var oldColor = GUI.contentColor;

            GUI.contentColor = Color.black;
            for (int i = layers.Count - 1; i >= 0; --i)
            {
                bool userToggle = EditorGUILayout.ToggleLeft(layers[i].ToString(), toggles_[i]);

                if (userToggle != toggles_[i])
                {
                    map.SetLayerVisible(layers[i], userToggle);
                    toggles_[i] = userToggle;
                }
            }
            GUI.color = oldColor;

            EditorGUI.indentLevel--;
        }

    }
}