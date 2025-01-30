using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ManipulationType_
{
    everything = 0,
    oneHanded = 1,
    twoHanded = 2,
    nothing = 3
}

public class OverrideManipulationType : MonoBehaviour
{
    // Start is called before the first frame update

    private ObjectManipulator om = null;
    public ManipulationType_ manipulationType = ManipulationType_.everything;

    void Start()
    {
        om = GetComponent<ObjectManipulator>();

        if (om != null)
        {
            if (manipulationType == ManipulationType_.oneHanded)
            {
                om.ManipulationType = Microsoft.MixedReality.Toolkit.Utilities.ManipulationHandFlags.OneHanded;
            }
            else if (manipulationType == ManipulationType_.twoHanded)
            {
                om.ManipulationType = Microsoft.MixedReality.Toolkit.Utilities.ManipulationHandFlags.TwoHanded;
            }else if (manipulationType == ManipulationType_.everything)
            {
                om.ManipulationType = Microsoft.MixedReality.Toolkit.Utilities.ManipulationHandFlags.OneHanded | Microsoft.MixedReality.Toolkit.Utilities.ManipulationHandFlags.TwoHanded;
            }
            else if (manipulationType == ManipulationType_.nothing)
            {
                om.ManipulationType = ~(Microsoft.MixedReality.Toolkit.Utilities.ManipulationHandFlags.OneHanded | Microsoft.MixedReality.Toolkit.Utilities.ManipulationHandFlags.TwoHanded);
            }
            else
            {
                Debug.LogError("Invalid Manipulation Type.");
            }
        }
        else
        {
            Debug.LogError("No Object Manipulator Found.");
        }
    }

}
