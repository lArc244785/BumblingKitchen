using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace BumblingKitchen
{
    public class CharacterSpawner : MonoBehaviour
    {
        [SerializeField] private List<Transform> _spawnPoint;
        [SerializeField] private List<NetworkPrefabRef> _playableCharacters;


        public void SpawnPlayerCharacter(NetworkRunner runner,int playerIndex, int characterIndex, NetworkRunner.OnBeforeSpawned onBeforeSpawned)
		{
            Vector3 spawnPoint = _spawnPoint[playerIndex].position;
            NetworkPrefabRef playerPrefab = _playableCharacters[characterIndex];

            var player = runner.Spawn(
                            playerPrefab,
                            spawnPoint,
                            Quaternion.identity,
                            runner.LocalPlayer,
                            onBeforeSpawned); ;
        }
    }
}
