using System.Collections.Generic;
using Animation;
using UnityEngine;

namespace Core.Unit.Model
{
    /// <summary>
    /// Handles storing different transforms that make up a model
    /// </summary>
    public class ModelContainer : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] private List<GameObject> listOfChildren;
    
        public AnimationContainer animContainer;
    
        public TargetingType type;
        private void Start()
        {
            GetChildRecursive(gameObject);
        }

        private void GetChildRecursive(GameObject obj)
        {
            if (null == obj)
                return;

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                    continue;
                listOfChildren.Add(child.gameObject);
                GetChildRecursive(child.gameObject);
            }
        }
    }
}