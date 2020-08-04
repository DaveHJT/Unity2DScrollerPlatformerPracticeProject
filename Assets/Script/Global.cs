using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomExtensions;
public class Global : MonoBehaviour
{
}
namespace CustomExtensions {
public static class Vector3Extension {
    public static Vector2 toVector2(this Vector3 vec3) {
        return new Vector2(vec3.x, vec3.y);
    }
}
}