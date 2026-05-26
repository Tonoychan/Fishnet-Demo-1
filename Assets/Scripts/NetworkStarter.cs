using FishNet;
using UnityEngine;

public class NetworkStarter : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 220, 80));

        if (!InstanceFinder.IsServerStarted && !InstanceFinder.IsClientStarted)
        {
            if (GUILayout.Button("Start Host (Server + Client)"))
            {
                InstanceFinder.ServerManager.StartConnection();
                InstanceFinder.ClientManager.StartConnection();
            }

            if (GUILayout.Button("Start Client Only"))
            {
                InstanceFinder.ClientManager.StartConnection();
            }
        }
        else
        {
            GUILayout.Label("Connected!");
        }

        GUILayout.EndArea();
    }
}