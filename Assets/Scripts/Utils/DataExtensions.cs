using UnityEngine;

namespace Utils
{
    public static class DataExtensions
    {
        public static float SqrMagnitudeTo(this Vector3 from, Vector3 to) => 
            Vector3.SqrMagnitude(to - from);
      
    }
}