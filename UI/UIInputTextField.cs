using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace SafeChests.UI
{
    public class UIInputTextField : UIPanel
    {
        private string _text;
        private string _hintText;
        private bool _focused;

        public UIInputTextField(string text, string hintText)
        {
            _text = text ?? "";
            _hintText = hintText ?? "";
            SetPadding(8f);
            BackgroundColor = Color.Gray * 0.8f;
            BorderColor = Color.Black;

            OnLeftClick += (evt, element) =>
            {
                _focused = true;
                Main.clrInput();
            };

            OnRightClick += (evt, element) =>
            {
                _focused = false;
            };
        }

        public string Text
        {
            get => _text;
            set => _text = value;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            string displayText = _focused ? _text + "_" : _text;
            if (string.IsNullOrEmpty(_text) && !_focused)
                displayText = _hintText;

            Vector2 pos = GetInnerDimensions().Position();
            pos.Y += 2;
            pos.X += 4;

            if (_focused)
            {
                Terraria.GameInput.PlayerInput.WritingText = true;
                Main.instance.HandleIME();
                string newText = Main.GetInputText(_text);
                if (newText != _text)
                {
                    _text = newText;
                }
            }

            Utils.DrawBorderString(spriteBatch, displayText, pos, Color.White, 0.8f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Verificar si el mouse está fuera de este elemento y se hace clic
            if (_focused && Main.mouseLeft && !ContainsPoint(new Vector2(Main.mouseX, Main.mouseY)))
            {
                _focused = false;
            }
            
            // Si este elemento está enfocado, asegurarse de que PlayerInput esté habilitado
            if (_focused)
            {
                Terraria.GameInput.PlayerInput.WritingText = true;
            }
        }

        public override void OnDeactivate()
        {
            _focused = false;
            base.OnDeactivate();
        }
    }
}