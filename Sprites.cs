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
            public Sprites() {
            }

            MySprite _spriteText = new MySprite() {
                Type = SpriteType.TEXT,
                Data = "Line 1",
                RotationOrScale = 0.8f,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = "White"
            };
            MySprite _spriteCursor = new MySprite() {
                Type = SpriteType.TEXTURE,
                Data = "AH_TextBox",
                Size = new Vector2(600, 30),
                Color = Color.White.Alpha(0.26f),
                Alignment = TextAlignment.CENTER
            };
            MySprite _spriteCircleLG = new MySprite() {
                Type = SpriteType.TEXTURE,
                Data = "CircleHollow",
                Size = new Vector2(50, 50),
                Color = Color.White.Alpha(0.36f),
                Alignment = TextAlignment.CENTER
            };
            MySprite _spriteCircleSM = new MySprite() {
                Type = SpriteType.TEXTURE,
                Data = "Circle",
                Size = new Vector2(10, 10),
                Color = Color.White,
                Alignment = TextAlignment.CENTER
            };
            public MySprite SpriteText {
                get { return _spriteText; }
                set { _spriteText = value; }
            }
            public MySprite SpriteCursor {
                get { return _spriteCursor; }
                set { _spriteCursor = value; }
            }
            public MySprite SpriteCircleLG {
                get { return _spriteCircleLG; }
                set { _spriteCircleLG = value; }
            }
            public MySprite SpriteCircleSM {
                get { return _spriteCircleSM; }
                set { _spriteCircleSM = value; }
            }

        }
    }
}
