using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangServer
{
    using FreeNet;

    // 객체로서의 플레이어가 아니라, 접속한 개개인을 가리키는 것으로 추정
    public class Player
    {
        GameUser owner;
        public byte player_index { get; private set; }
        public int playerFlag;      // 플레이어 지정 번호

        public Player(GameUser user, byte player_index)
        {
            this.owner = user;
            this.player_index = player_index;
        }

        public void send(CPacket msg)
        {
            this.owner.send(msg);
            CPacket.destroy(msg);
        }

        public void send_for_broadcast(CPacket msg)
        {
            this.owner.send(msg);
        }
    }
}
