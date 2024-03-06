using System.Collections.Generic;
using UnityEngine;

namespace SuperTiled2Unity
{
    internal class ImportErrors : ScriptableObject
    {
        [SerializeField]
        private List<string> m_Errors;

        public void AddError(string error)
        {
            m_Errors.Add(error);
        }
    }
}
