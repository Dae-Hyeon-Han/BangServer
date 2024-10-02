using System;
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

        #region 여기서부터 뱅 전용 프로토콜
        // 누군가를 사격할 경우
        CHARACTERCHOICE = 11,

        // 카드 사용(string과 연계하여 사용할 것)
        USECARD = 12,

        SHOT_REQ = 13,

        // 인디언 사용
        INDIANS_REQ = 15,

        // 기관총 사용
        MACHINE_GUN_REQ = 17,

        // 결투 사용
        DUEL_REQ = 19,

        #endregion
    }
}
