using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace DefaultNamespace
{
    [Serializable]
    public class DataObject
    {
        public Entry project;

        public Entry Project
        {
            get => project;
            set => project = value;
        }
    }

    public class Entry
    {
        public List<Entry> files;
        public int numberOfLines;
        public int numberOfMethods;
        public int numberOfAbstractClasses;
        public int numberOfInterfaces;
        public string name;
        public string type;
        public float x;
        public float z;
        public float w;
        public float h;
        public int depth;
        public GridObjectCollection goc;
        public Entry parentEntry;
        public Color color;
    }
}