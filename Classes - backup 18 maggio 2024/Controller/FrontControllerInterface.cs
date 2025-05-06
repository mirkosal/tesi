using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface FrontControllerInterface
{
    object InputController(string[] stringArray = null, int[] intArray = null, float[] floatArray = null);
}
