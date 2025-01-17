﻿using ChessApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WebSocketManager;
using WebSocketManager.Common;

namespace ChessApp.GameLogic
{
    public class GameHandler : WebSocketHandler
    {
        public GameHandler(WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager)
        {
        }


        public async Task MakeMove(string u, string gK, string xy1xy2)
        {
            var user = JsonConvert.DeserializeObject<User>(u);
            await GameManager.Instance.TryMove(user, gK, xy1xy2);
        }

        public async Task ConnectedGame(string socketId, string serializedUser, string serializedOptions)
        {
            var user = JsonConvert.DeserializeObject<User>(serializedUser);
            var options = JsonConvert.DeserializeObject<GameOptions>(serializedOptions);
            await GameManager.Instance.ProccessConnection(user, options, socketId);           
        }

        public async Task SendMessage(string serializedUser, string gameId, string message)
        {
            var user = JsonConvert.DeserializeObject<User>(serializedUser);
            await GameManager.Instance.ProccessMessage(user, gameId, message);
        }

        public async Task DisconnectedGame(string socketId)
        {
            foreach (var g in GameManager.Instance.Games)
            {
                if (g.Value.hostCS == socketId)
                {
                    g.Value.history += GameManager.Instance.GetLogMove(g.Value.host.login + " disconnect");
                    g.Value.hostCS = null;
                }
                else if (g.Value.guestCS == socketId)
                {
                    g.Value.history += GameManager.Instance.GetLogMove(g.Value.guest.login + " disconnect");
                    g.Value.guestCS = null;
                }
            }            
        }

        public async Task OnMove(string socketId, string gameData)
        {
            var game = JsonConvert.DeserializeObject<Game>(gameData);
            GameManager.Instance.Games.TryGetValue(game.id, out Game exists);
            if (exists != null)
            {
                //exists.data = game.data;
                //exists.y = game.y;
                //exists.tail = game.tail;
                //exists.trail = game.trail;
            }
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);

            var socketId = WebSocketConnectionManager.GetId(socket);
            
            var message = new Message
            {
                MessageType = MessageType.Text,
                Data = $"Game with socket id :{socketId} is now connected!"
            };

            await SendMessageToAllAsync(message);
        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            await base.OnDisconnected(socket);

            var socketId = WebSocketConnectionManager.GetId(socket);
            await DisconnectedGame(socketId);
            var message = new Message
            {
                MessageType = MessageType.Text,
                Data = $"Game with socket id :{socketId} is now disconnected!"
            };

            await SendMessageToAllAsync(message);           
        }
    }
}
