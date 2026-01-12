using UnityEngine;
using UnityEditor;
using PlaceholderHack.Core;
using System.Collections.Generic;

public class DataTest
{
    [MenuItem("HardStakes/Test Data Contract")]
    public static void VerifyHandshake()
    {
        List<byte> mock = new List<byte>();

        // 1. Discriminator (8)
        mock.AddRange(new byte[8]);

        // 2. Player One (32)
        mock.AddRange(new byte[32]);

        // 3. Player Two (Option 1 + 32)
        mock.Add(0); // No P2 yet
        mock.AddRange(new byte[32]);

        // 4. P1 Coords [-200, 0] (16)
        mock.AddRange(System.BitConverter.GetBytes((long)-200));
        mock.AddRange(System.BitConverter.GetBytes((long)0));

        // 5. P2 Coords [200, 0] (16)
        mock.AddRange(System.BitConverter.GetBytes((long)200));
        mock.AddRange(System.BitConverter.GetBytes((long)0));

        // 6. Radius (8)
        mock.AddRange(System.BitConverter.GetBytes((ulong)500));

        // 7. Status (1)
        mock.Add(0);

        // 8. Winner (Option 1 + 32)
        mock.Add(0);
        mock.AddRange(new byte[32]);

        // 9. Frame (8)
        mock.AddRange(System.BitConverter.GetBytes((ulong)1));

        Debug.Log($"Simulating Byte Packet Size: {mock.Count} bytes");

        // Attempt Deserialize
        var state = GameStateAccount.Deserialize(mock.ToArray());

        if (state != null && state.P1Coords[0] == -200)
        {
            Debug.Log("<color=green>✅ PASSED: C# Logic matches Rust Struct.</color>");
        }
        else
        {
            Debug.LogError("❌ FAILED: Struct mismatch.");
        }
    }
}