﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangServer
{
    using FreeNet;

    public class GameRoom
    {
        enum PLAYER_STATE : byte
        {
            // 방에 막 입장한 상태
            ENTERED_ROOM,

            // 로딩을 완료한 상태
            LOADING_COMPLETE,

            // 턴 진행 준비 상태
            READY_TO_TURN,

            // 턴 연출을 모두 완료한 상태
            CLIENT_TURN_FINISHED
        }

        // 게임을 진행하는 플레이어 7명이 존재한다.
        List<Player> players;

        // 플레이어들의 상태를 관리하는 변수
        Dictionary<byte, PLAYER_STATE> playerState;

        // 현재 턴을 진행하고 있는 플레이어의 인덱스
        byte currentTurnPlayer;

        // 게임 카드 목록
        List<short> playingCard;

        // 게임 캐릭터 목록
        List<short> charCard;

        public GameRoom()
        {
            this.players = new List<Player>();
            this.playerState = new Dictionary<byte, PLAYER_STATE>();
            this.currentTurnPlayer = 0;

            // 카드 덱 구성하는 코드 작성
        }

        /// <summary>
        /// 모든 유저들에게 메시지를 전송함
        /// 채팅 내역, 개별 플레이어의 정보(라이프, 잔여 카드 수, (시작 시) 선택한 캐릭터 등)
        /// </summary>
        /// <param name="msg"></param>
        void BroadCast(CPacket msg)
        {
            this.players.ForEach(player => player.send_for_broadcast(msg));
            CPacket.destroy(msg);
        }

        /// <summary>
        /// 플레이어의 상태를 변경한다.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="state"></param>
        void ChangePlayerState(Player player, PLAYER_STATE state)
        {
            if (this.playerState.ContainsKey(player.player_index))
                this.playerState[player.player_index] = state;
            else
                this.playerState.Add(player.player_index, state);
        }

        /// <summary>
        /// 모든 플레이어가 특정 상태가 되었는지를 판단.
        /// 모든 플레이어가 같은 상태에 있다면 true, 한명이라도 다른 상태에 있다면 false를 리턴
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        bool AllPlayersReady(PLAYER_STATE state)
        {
            foreach(KeyValuePair<byte, PLAYER_STATE> kvp in this.playerState)
            {
                if (kvp.Value != state)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 매칭이 성사된 플레이어들이 게임에 입장한다.
        /// 추후 7인용으로 늘릴 것.
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        public void EnterGameRoom(GameUser user1, GameUser user2)
        {
            // 플레이어들을 생성하고 각각 0번, 1번 인덱스를 부여해준다.
            Player player1 = new Player(user1, 0);
            Player player2 = new Player(user2, 1);
            
            // 이 리스트를 기반으로 유저간 거리 측정할 때 쓸 것.
            this.players.Clear();
            this.players.Add(player1);
            this.players.Add(player2);

            // 플레이어들의 초기 상태를 지정해 준다.
            this.playerState.Clear();
            ChangePlayerState(player1, PLAYER_STATE.ENTERED_ROOM);
            ChangePlayerState(player2, PLAYER_STATE.ENTERED_ROOM);

            //player1.playerFlag = 0;
            //player2.playerFlag = 1;

            // 로딩 시작 메시지 전송
            this.players.ForEach(player =>
            {
                CPacket msg = CPacket.create((Int16)BangProtocol.START_LOADING);
                msg.push(player.player_index);      // 본인의 플레이어 인덱스를 알려줌
            });

            user1.EnterRoom(player1, this);
            user2.EnterRoom(player2, this);
        }

        public void LoadingComplete(Player player)
        {
            // 해당 플레이어를 로딩 완료 상태로 변경
            ChangePlayerState(player, PLAYER_STATE.LOADING_COMPLETE);

            // 모든 유저가 준비 상태인지 체크
            if (!AllPlayersReady(PLAYER_STATE.LOADING_COMPLETE))
                return;

            // 모두 준비 되었다면 게임 시작
            BattleStart();
        }

        /// <summary>
        /// 게임을 시작한다.
        /// </summary>
        void BattleStart()
        {
            // 게임을 새로 시작할 때마다 초기화 해줘야 할 것들.
            ResetGameData();

            // 게임 시작 메시지 전송
            CPacket msg = CPacket.create((short)BangProtocol.GAME_START);

            // 플레이어에게 두장의 캐릭터 중 하나 택할 것을 요구

            // 첫턴을 진행할 플레이어 인덱스
            msg.push(this.currentTurnPlayer);
            BroadCast(msg);
        }

        /// <summary>
        /// 턴을 시작하라고 클라이언트들에게 알려준다.
        /// </summary>
        void StartTurn()
        {
            this.players.ForEach(player => ChangePlayerState(player, PLAYER_STATE.READY_TO_TURN));
            CPacket msg = CPacket.create((short)BangProtocol.START_PLAYER_TURN);
            msg.push(this.currentTurnPlayer);
            BroadCast(msg);
        }

        /// <summary>
        /// 게임 데이터 초기화
        /// </summary>
        void ResetGameData()
        {

        }

        /// <summary>
        /// 플레이어 인덱스에 해당하는 플레이어를 구한다.
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        Player GetPlayer(byte playerIndex)
        {
            return this.players.Find(obj => obj.player_index == playerIndex);
        }

        /// <summary>
        /// 현재 턴인 플레이어의 상대 플레이어를 구한다?
        /// </summary>
        /// <returns></returns>
        Player GetOpponentPlayer()
        {
            return this.players.Find(player => player.player_index != this.currentTurnPlayer);
        }

        /// <summary>
		/// 현재 턴을 진행중인 플레이어를 구한다.
		/// </summary>
		/// <returns></returns>
		Player GetCurrentPlayer()
        {
            return this.players.Find(player => player.player_index == this.currentTurnPlayer);
        }

        /// <summary>
        /// 클라이언트의 bang 공격 요청
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="TargetPlayer"></param>
        public void ShotPlayer(Player Attacker, Player Target, short AttackRange)
        {

        }

        /// <summary>
        /// 공격자의 사정거리 안에 타깃이 존재하는지 체크
        /// </summary>
        /// <param name="Attacker">공격자</param>
        /// <param name="Target">피격자</param>
        /// <param name="AttackRange">공격자의 공격 범위</param>
        /// <returns></returns>
        bool IsAttack(Player Attacker, Player Target, short AttackRange)
        {
            return true;
        }




        public void destroy()
        {
            CPacket msg = CPacket.create((short)BangProtocol.ROOM_REMOVED);
            BroadCast(msg);

            this.players.Clear();
        }
    }
}
