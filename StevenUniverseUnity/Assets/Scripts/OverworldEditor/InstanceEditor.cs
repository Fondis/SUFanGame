﻿using UnityEngine;
using StevenUniverse.FanGame.Overworld;

namespace StevenUniverse.FanGame.OverworldEditor
{
    [ExecuteInEditMode]
    public abstract class InstanceEditor : MonoBehaviour
    {
        private Vector3 lastPosition;

        public abstract Instance Instance { get; }

        protected virtual void Start()
        {
            UpdateLastPosition();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (transform.position != lastPosition)
            {
                Instance.Position = GetRelevantPosition();
                UpdateLastPosition();
            }
        }

        private void UpdateLastPosition()
        {
            lastPosition = GetRelevantPosition();
        }

        private Vector3 GetRelevantPosition()
        {
            //If the Instance is not in a GroupInstance, use the global position
            if (transform.parent == null || transform.parent.GetComponent<GroupInstanceEditor>() == null)
            {
                return transform.position;
            }
            //Otherwise, use the local position
            else
            {
                return transform.localPosition;
            }
        }

        public int Elevation
        {
            get { return Instance.Elevation; }
            set { Instance.Elevation = value; }
        }
    }
}