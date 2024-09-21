using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangServer
{
    public abstract class Characters
    {
        protected string name;        // 캐릭터 이름
        protected int life;           // 기본 목숨
        //string coments;     // 캐릭터에 대한 설명. 클라에서 처리

        public abstract void CharacterSettings();
    }

    public enum Job
    {
        SHERIFF = 0,
        VICE = 1,
        OUTLAW = 2,
        RENEGADE = 3
    }

    public enum Charater
    {
        Willy_The_Kid = 0,      //
        Clamity_Janet = 1,      //
        Kit_Carlson = 2,        //
        Bart_Cassidy = 3,       //
        Sid_Ketchum = 4,
        Lucky_Duke = 5,         //
        Jourdonnais = 6,        //
        Black_Jack = 7,         //
        Vulture_Sam = 8,        //
        Jesse_Jones = 9,        //
        Suzy_Lafayette = 10,    //
        Pedro_Ramirez = 11,     //
        Slab_The_Killer = 12,   //
        Rose_Doolan = 13,       //
        Paul_Regret = 14,       //
        El_Gringo = 15          //
    }

    class Willy_The_Kid : Characters
    {
        public override void CharacterSettings()
        {
            //"윌리 더 키드"
            name = "Willy_The_Kid";
            life = 4;
        }
    }

    class Clamity_Janet : Characters
    {
        public override void CharacterSettings()
        {
            // "캘러미티 자넷"
            name = "Clamity_Janet";
            life = 4;
        }
    }

    class Kit_Carlson : Characters
    {
        public override void CharacterSettings()
        {
            // "키트 칼슨"
            name = "Kit_Carlson";
            life = 4;
        }
    }

    class Bart_Cassidy : Characters
    {
        public override void CharacterSettings()
        {
            // "바트 캐시디"
            name = "Bart_Cassidy";
            life = 4;
        }
    }

    class Sid_Ketchum : Characters
    {
        public override void CharacterSettings()
        {
            // "시드 케첨"
            name = "Sid_Ketchum";
            life = 4;
        }
    }

    class Lucky_Duke : Characters
    {
        public override void CharacterSettings()
        {
            // "럭키 듀크"
            name = "Lucky_Duke";
            life = 4;
        }
    }

    class Jourdonnais : Characters
    {
        public override void CharacterSettings()
        {
            // "주르도네"
            name = "Jourdonnais";
            life = 4;
        }
    }

    class Black_Jack : Characters
    {
        public override void CharacterSettings()
        {
            // "블랙 잭"
            name = "Black_Jack";
            life = 4;
        }
    }

    class Vulture_Sam : Characters
    {
        public override void CharacterSettings()
        {
            // "벌쳐 샘"
            name = "Vulture_Sam";
            life = 4;
        }
    }

    class Jesse_Jones : Characters
    {
        public override void CharacterSettings()
        {
            // "제시 존스"
            name = "Jesse_Jones";
            life = 4;
        }
    }

    class Suzy_Lafayette : Characters
    {
        public override void CharacterSettings()
        {
            // "수지 라파예트"
            name = "Suzy_Lafayette";
            life = 4;
        }
    }

    class Pedro_Ramirez : Characters
    {
        public override void CharacterSettings()
        {
            // "페드로 라미레즈"
            name = "Pedro_Ramirez";
            life = 4;
        }
    }

    class Slab_The_Killer : Characters
    {
        public override void CharacterSettings()
        {
            // "슬랩 더 킬러"
            name = "Slab_The_Killer";
            life = 4;
        }
    }

    class Rose_Doolan : Characters
    {
        public override void CharacterSettings()
        {
            // "로즈 둘란"
            name = "Rose_Doolan";
            life = 4;
        }
    }

    class Paul_Regret : Characters
    {
        public override void CharacterSettings()
        {
            // "폴 리그레트"
            name = "Paul_Regret";
            life = 3;
        }
    }

    class El_Gringo: Characters
    {
        public override void CharacterSettings()
        {
            // "엘 그링고"
            name = "El_Gringo";
            life = 3;
        }
    }
}
