using System;
using System.Linq;
using DefaultNamespace;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace CityAR
{
    public class VisualizationCreator : MonoBehaviour
    {
        public GameObject districtPrefab;
        public GameObject buildingPrefab;
        private DataObject _dataObject;
        private GameObject _platform;
        private Data _data;

        private void Start()
        {
            _platform = GameObject.Find("Platform");
            _data = _platform.GetComponent<Data>();
            _dataObject = _data.ParseData();
            BuildCity(_dataObject);
        }

        private void BuildCity(DataObject p)
        {
            if (p.project.files.Count <= 0) return;
            p.project.w = 1;
            p.project.h = 1;
            p.project.depth = 1;
            BuildDistrict(p.project, false);
        }

        /*
         * entry: Single entry from the data set. This can be either a folder or a single file.
         * splitHorizontal: Specifies whether the subsequent children should be split horizontally or vertically along the parent
         */
        private void BuildDistrict(Entry entry, bool splitHorizontal)
        {
            if (entry.type.Equals("File"))
            {
                //TODO if entry is from type File, create building
                entry.depth = entry.parentEntry.depth;
                entry.x = entry.parentEntry.x;
                entry.z = entry.parentEntry.z;
                entry.w = 0.04f;
                entry.h = 0.04f;
                BuildBuilding(entry);
            }
            else
            {
                var x = entry.x;
                var z = entry.z;

                float dirLocs = entry.numberOfLines;
                entry.color = GetColorForDepth(entry.depth);

                BuildDistrictBlock(entry, false);

                foreach (var subEntry in entry.files)
                {
                    subEntry.x = x;
                    subEntry.z = z;

                    if (subEntry.type.Equals("Dir"))
                    {
                        float ratio = subEntry.numberOfLines / dirLocs;
                        subEntry.depth = entry.depth + 1;

                        if (splitHorizontal)
                        {
                            subEntry.w = ratio * entry.w; // split along horizontal axis
                            subEntry.h = entry.h;
                            x += subEntry.w;
                        }
                        else
                        {
                            subEntry.w = entry.w;
                            subEntry.h = ratio * entry.h; // split along vertical axis
                            z += subEntry.h;
                        }
                    }
                    else
                    {
                        subEntry.parentEntry = entry;
                    }

                    BuildDistrict(subEntry, !splitHorizontal);
                }

                if (!splitHorizontal)
                {
                    entry.x = x;
                    entry.z = z;
                    if (ContainsDirs(entry))
                    {
                        entry.h = 1f - z;
                    }

                    entry.depth += 1;
                    BuildDistrictBlock(entry, true);
                }
                else
                {
                    entry.x = -x;
                    entry.z = z;
                    if (ContainsDirs(entry))
                    {
                        entry.w = 1f - x;
                    }

                    entry.depth += 1;
                    BuildDistrictBlock(entry, true);
                }
            }
        }

        private void BuildBuilding(Entry entry)
        {
            var buildingFabInstance = Instantiate(buildingPrefab, _platform.transform, true);
            if (entry.name.Equals("AssetLoader.java"))
            {
                print("AssetLoader.java");
                // buildingFabInstance.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
            }
            buildingFabInstance.name = entry.name;
            var scale = new Vector3(entry.w, entry.numberOfLines * 0.1f,entry.h);
            var scaleX = scale.x - (entry.depth * 0.005f);
            var scaleZ = scale.z - (entry.depth * 0.005f);
            buildingFabInstance.transform.localScale = new Vector3(scaleX, scale.y, scaleZ);
        }

        /*
         * entry: Single entry from the data set. This can be either a folder or a single file.
         * isBase: If true, the entry has no further subfolders. Buildings must be placed on top of the entry
         */
        private void BuildDistrictBlock(Entry entry, bool isBase)
        {
            if (entry == null)
            {
                return;
            }

            var w = entry.w; // w -> x coordinate
            var h = entry.h; // h -> z coordinate

            if (!(w * h > 0))
            {
                return;
            }

            var prefabInstance = Instantiate(districtPrefab, _platform.transform, true);
            prefabInstance.transform.localScale = new Vector3(entry.w, 1f, entry.h);
            prefabInstance.transform.localPosition = new Vector3(entry.x, entry.depth, entry.z);
            var scale = prefabInstance.transform.localScale;
            var scaleX = scale.x - (entry.depth * 0.005f);
            var scaleZ = scale.z - (entry.depth * 0.005f);
            var shiftX = (scale.x - scaleX) / 2f;
            var shiftZ = (scale.z - scaleZ) / 2f;
                
                
            if (!isBase)
            {
                prefabInstance.name = entry.name;
                prefabInstance.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = entry.color;
            }
            else
            {
                prefabInstance.name = entry.name + "Base";
                prefabInstance.transform.GetChild(0).rotation = Quaternion.Euler(90, 0, 0);
                prefabInstance.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                Debug.Log("Collection updated");
                foreach (var subEntry in entry.files.Where(subEntry => subEntry.type.Equals("File")))
                {
                    GameObject.Find(subEntry.name).transform.parent = prefabInstance.transform.GetChild(0).transform;
                }
                var objectCollection = prefabInstance.transform.GetChild(0).GetComponent<GridObjectCollection>();
                if (scaleX <= scaleZ)
                {
                    objectCollection.Layout = 0;
                    objectCollection.CellWidth = 0.3f;
                    objectCollection.CellHeight = 0.1f;
                    objectCollection.Columns = Math.Min((int)Math.Round(scaleX / 0.04f), 4); 
                }
                else
                {
                    objectCollection.Rows = Math.Min((int)Math.Round(scaleZ / 0.04f), 4);
                }
                objectCollection.UpdateCollection();
                prefabInstance.transform.GetChild(0).localPosition = new Vector3(-0.5f, 0.5f, 0.5f);
            }

            prefabInstance.transform.localScale = new Vector3(scaleX, scale.y, scaleZ);
            var position = prefabInstance.transform.localPosition;
            prefabInstance.transform.localPosition =
                new Vector3(position.x - shiftX, position.y, position.z + shiftZ);
        }

        private static bool ContainsDirs(Entry entry)
        {
            return entry.files.Any(e => e.type.Equals("Dir"));
        }

        private static Color GetColorForDepth(int depth)
        {
            Color color = depth switch
            {
                1 => new Color(179f / 255f, 209f / 255f, 255f / 255f),
                2 => new Color(128f / 255f, 179f / 255f, 255f / 255f),
                3 => new Color(77f / 255f, 148f / 255f, 255f / 255f),
                4 => new Color(26f / 255f, 117f / 255f, 255f / 255f),
                5 => new Color(0f / 255f, 92f / 255f, 230f / 255f),
                _ => new Color(0f / 255f, 71f / 255f, 179f / 255f)
            };

            return color;
        }
    }
}