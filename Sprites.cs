using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript {
    partial class Program {
        public class Sprites {
            Program _program;
            public Sprites(Program program) {
                _program = program;
            }

            MySprite _spriteText = new MySprite() {
                Type = SpriteType.TEXT,
                Data = "Line 1",
                RotationOrScale = 0.8f,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = "White"
            };
            MySprite _spriteTexture = new MySprite() {
                Type = SpriteType.TEXTURE,
                Data = "AH_TextBox",
                Size = new Vector2(256, 30),
                Color = Color.White.Alpha(0.26f),
                Alignment = TextAlignment.CENTER
            };
            public MySprite SpriteText {
                get { return _spriteText; }
                set { _spriteText = value; }
            }
            public MySprite SpriteTexture {
                get { return _spriteTexture; }
                set { _spriteTexture = value; }
            }


        }
    }
}
