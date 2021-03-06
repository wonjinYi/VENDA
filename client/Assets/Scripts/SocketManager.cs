﻿using UnityEngine;
using socket.io;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

public class SocketManager : MonoBehaviour
{
    [SerializeField]
    private Character prefab;

    private Dictionary<string, string> keylist = new Dictionary<string, string>();
    private bool connected = false;
    public Character player;
    [SerializeField]
    private string serverUrl = "http://59.11.136.225:5353/";
    public Socket socket;
    private string id;
    public Dictionary<string,Character> characterList = new Dictionary<string, Character>();



    void Start()
    {
        Application.targetFrameRate = 60;
        socket = Socket.Connect(serverUrl);


        socket.On(SystemEvents.connect, () =>
        {
            connected = true;
        });

        socket.On(SystemEvents.reconnect, (int reconnectAttempt) =>
        {
            Debug.Log("Hello, Again! " + reconnectAttempt);
        });

        socket.On(SystemEvents.disconnect, () =>
        {
            Debug.Log("Bye~");
            connected = false;
        });

        socket.On("userData", (string data) =>
        {
            var userData = JsonConvert.DeserializeObject<Dictionary<string, CharacterData>>(data);
            foreach (var keyValue in userData)
            {
                if(keyValue.Key == id)
                    continue;

                if(!characterList.ContainsKey(keyValue.Key))
                {
                    var newChar = Instantiate<Character>(prefab);
                    characterList.Add(keyValue.Key,newChar);
                }


                Character cha = characterList[keyValue.Key];
                cha.SetData(keyValue.Value);
            }

        });

        socket.On("info", (string data) => {
            id = (string)JObject.Parse(data)["id"];
            player.setId(id);
        });


        socket.On("hit",(string data)=>{
            int dmg = (int)JObject.Parse(data)["dmg"];
            player.GetDmg(dmg);
            Debug.Log("맞앗어");
        });

        socket.On("animate",(string data)=>{
            string id_ = (string)JObject.Parse(data)["id"];
            int animeId = (int)JObject.Parse(data)["animeId"];
            if (id_ != id) {
                characterList[id_].PlayAnimation(animeId);
            }
        });
        socket.On("delPlayer",(string data)=>{
            string id_ = (string)JObject.Parse(data)["id"];
            Destroy(characterList[id_].gameObject);
            characterList.Remove(id_);
        });
    }

    void Update()
    {
        if (connected)
        {
            socket.EmitJson("myData", JsonConvert.SerializeObject(player.GetData()));
        }

    }
}
