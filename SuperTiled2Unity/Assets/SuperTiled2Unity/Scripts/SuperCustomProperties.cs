﻿using System.Collections.Generic;
using UnityEngine;

namespace SuperTiled2Unity
{
    // A simple component for storing custom properites
    // Ideally, custom import scripts should use these in the import process and then strip them from the final prefab
    public class SuperCustomProperties : MonoBehaviour
    {
        public List<CustomProperty> m_Properties;

        public bool TryGetCustomProperty(string name, out CustomProperty property)
        {
            return m_Properties.TryGetProperty(name, out property);
        }

        public void RemoveCustomProperty(string name) {
            m_Properties.RemoveAll(PredicateRemoveCustomProperty);

            bool PredicateRemoveCustomProperty(CustomProperty customProperty) {
                return name.Equals(customProperty.m_Name);
            }
        }
    }
}
