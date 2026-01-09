using UnityEngine;
using UnityEditor;
using PlaceholderHack.Core;
using System.Collections.Generic;

public class DataTest
{
    [MenuItem("HardStakes/Test Data Contract")]
    public static void VerifyHandshake()
    {
        // 1. Simulate Raw Bytes from Solana (What Rust would send)
        // We manually construct a byte array that mimics the Rust layout
        List<byte> mockData = new List<byte>();

        // 8 bytes Discriminator (Fake)
        mockData.AddRange(new byte[8]);

        // P1 Coords: [-200, 0]
        mockData.AddRange(System.BitConverter.GetBytes((long)-200));
        mockData.AddRange(System.BitConverter.GetBytes((long)0));

        // P2 Coords: [200, 0]
        mockData.AddRange(System.BitConverter.GetBytes((long)200));
        mockData.AddRange(System.BitConverter.GetBytes((long)0));

        // Radius: 500
        mockData.AddRange(System.BitConverter.GetBytes((ulong)500));

        // Status: Waiting (0)
        mockData.Add(0);

        // Frame: 1
        mockData.AddRange(System.BitConverter.GetBytes((ulong)1));

        // 2. Attempt to Deserialize using our Core Logic
        GameStateAccount account = GameStateAccount.Deserialize(mockData.ToArray());

        // 3. Validation
        if (account.P1Coords[0] == -200 && account.MapRadius == 500)
        {
            Debug.Log("<color=green>✅ SUCCESS: C# correctly deciphered the Rust Data Structure.</color>");
            Debug.Log($"P1 X-Position in Unity Units: {account.P1Coords[0] / 100.0f}");
        }
        else
        {
            Debug.LogError("❌ FAILURE: Data mismatch. Check struct alignment.");
        }
    }
}