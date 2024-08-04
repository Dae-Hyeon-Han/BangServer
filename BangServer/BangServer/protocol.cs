﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangServer
{
	public enum PROTOCOL : short
	{
        #region
        BEGIN = 0,

        // 로딩을 시작해라.
        START_LOADING = 1,

        LOADING_COMPLETED = 2,

        // 게임 시작.
        GAME_START = 3,

        // 턴 시작.
        START_PLAYER_TURN = 4,

        // 클라이언트의 이동 요청.
        MOVING_REQ = 5,

        // 플레이어가 이동 했음을 알린다.
        PLAYER_MOVED = 6,

        // 클라이언트의 턴 연출이 끝났음을 알린다.
        TURN_FINISHED_REQ = 7,

        // 상대방 플레이어가 나가 방이 삭제되었다.
        ROOM_REMOVED = 8,

        // 게임방 입장 요청.
        ENTER_GAME_ROOM_REQ = 9,

        // 게임 종료.
        GAME_OVER = 10,
        #endregion

        //BEGIN = 0,

        //// 로딩을 시작해라.
        //START_LOADING = 1,

        //LOADING_COMPLETED = 2,

        //// 게임 시작.
        //GAME_START = 3,

        //// 턴 시작.
        //START_PLAYER_TURN = 4,

        //// 클라이언트의 이동 요청.
        //MOVING_REQ = 5,

        //// 클라이언트의 턴 연출이 끝났음을 알린다.
        //TURN_FINISHED_REQ = 6,

        //// 모든 플레이어가 나가 방이 삭제되었다.
        //ROOM_REMOVED = 7,

        //// 게임방 입장 요청.
        //ENTER_GAME_ROOM_REQ = 8,

        //// 게임 종료.
        //GAME_OVER = 9,

        //// 그 밖에 뭐가 더 나와야 할 것 같은데...

        END
	}
}
