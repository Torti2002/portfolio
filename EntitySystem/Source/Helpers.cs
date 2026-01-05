using UnityEngine;

public class Helpers
{
    public static string GenerateGUID()
    {
        return System.Guid.NewGuid().ToString();
    }
}
