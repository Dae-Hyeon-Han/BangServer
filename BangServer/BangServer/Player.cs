using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangServer
{
    using FreeNet;

    class Player
    {
        CGameUser owner;
        public byte player_index { get; private set; }

        public Player(CGameUser user, byte player_index)
        {
            this.owner = user;
            this.player_index = player_index;
        }

        public void send(CPacket msg)
        {
            this.owner.send(msg);
            CPacket.destroy(msg);
        }
    }
}
