using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper
{
    public enum ButtonState : byte
    {
        Free = 0,
        Mine = 1,
        Number = 2,
    }
    public enum RightClickState : byte
    {
        Nothing = 0, Flag = 1
    }
    public class GameButton : Button
    {
        public ButtonState State { get; set; } = ButtonState.Free;
        public RightClickState RightClickState { get; private set; } = RightClickState.Nothing;
        public List<GameButton> Neighbors = new List<GameButton>();

        public GameButton(int width, int height)
        {
            Width = width;
            Height = height;
            MouseRightButtonUp += HandleRightClick;
        }

        public void SetNumberState()
        {
            if (Neighbors.Any(x => x.State == ButtonState.Mine) && State == ButtonState.Free)
            {
                State = ButtonState.Number;
            }
        }
        public void FillNeighbors(List<GameButton> buttons)
        {
            Neighbors.AddRange(buttons);
        }
        public void HandleRightClick(object sender, MouseButtonEventArgs e)
        {
            switch (RightClickState)
            {
                case RightClickState.Nothing:
                    RightClickState = RightClickState.Flag;
                    Content = "🚩";
                    break;
                case RightClickState.Flag:
                    RightClickState = RightClickState.Nothing;
                    Content = "";
                    break;
            }
        }
    }
}
