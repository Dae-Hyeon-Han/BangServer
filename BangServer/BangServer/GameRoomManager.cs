using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangServer
{
    public class GameRoomManager
    {
        List<GameRoom> rooms;

        public GameRoomManager()
        {
            this.rooms = new List<GameRoom>();
        }

        /// <summary>
        /// 매칭을 요청한 유저들을 넘겨 받아 게임방을 생성함.
        /// 추후 7인으로 확장할 것
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        public void CreateRoom(GameUser user1, GameUser user2)
        {
            // 게임 방을 생성하여 입장시킴
            GameRoom battleRoom = new GameRoom();
            battleRoom.EnterGameRoom(user1, user2);

            Console.WriteLine("방 생성!");

            // 방 리스트에 추가하여 관리
            this.rooms.Add(battleRoom);
        }

        public void RemoveRoom(GameRoom room)
        {
            room.destroy();
            this.rooms.Remove(room);
        }
    }
}
