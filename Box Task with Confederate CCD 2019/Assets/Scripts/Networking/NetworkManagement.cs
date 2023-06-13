namespace MQ.MultiAgent
{
    using UnityEngine;
    using Mirror;
    using System;
    using Tobii.Research;

    public class NetworkManagement : NetworkManager
    {
        public void StartupHost()
        {
            StartHost();
        }

        public void JoinGame()
        {
            StartClient();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            NetworkServer.RegisterHandler<CreateAvatarMessage>(OnCreateCharacter);
        }

        public override void OnStartClient()
        {
            GameObject.Find("Menu Camera").GetComponent<Camera>().enabled = false;
            base.OnStartClient();
        }

        public override void OnStopClient()
        {
            EyeTrackingOperations.Terminate();
            GameObject.Find("Menu Camera").GetComponent<Camera>().enabled = true;
        }

        #region Player Setup and Spawn
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            CreateAvatarMessage message = new CreateAvatarMessage
            {
                gender = TitleScreenData.Gender == "Female" ? CreateAvatarMessage.Gender.Female : CreateAvatarMessage.Gender.Male
            };

            conn.Send(message);
        }

        //See https://mirror-networking.com/docs/Guides/GameObjects/SpawnPlayerCustom.html
        private void OnCreateCharacter(NetworkConnection conn, CreateAvatarMessage message)
        {
            GameObject player = Instantiate(spawnPrefabs[(int)message.gender], Vector3.zero, Quaternion.identity);

            PlayerProperties properties = player.GetComponent<PlayerProperties>();

            if (numPlayers > 0) properties.playerID = 2;
            else properties.playerID = 1;

            NetworkServer.AddPlayerForConnection(conn, player);
        }
        #endregion
    }
}