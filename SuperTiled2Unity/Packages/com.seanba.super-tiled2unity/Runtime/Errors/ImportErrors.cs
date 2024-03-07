using System.Collections.Generic;
using UnityEngine;

namespace SuperTiled2Unity
{
    public class ImportErrors : ScriptableObject
    {
        [SerializeField]
        private List<string> m_Errors = new List<string>();

        public void AddError(string error)
        {
            m_Errors.Add(error);
        }
    }
}
