﻿using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using Ballz.Input;
using Ballz.Gui;
using Ballz.Messages;
using Ballz.Sound;
using Ballz.GameSession.Logic;

namespace Ballz.Logic
{
    /// <summary>
    ///     Logic control Processes Messages and other system reactions with regard to the current gamestate.
    ///     It uses Message events to inform relevant classes.
    /// </summary>
    public class LogicControl
    {
        private readonly Stack<MenuPanel> activeMenu = new Stack<MenuPanel>();
        private GameState state;
        private bool rawInput;
        private Ballz Game;
        private Timer ButtonRepeat, ButtonDelay;
        private ElapsedEventHandler repeatHandler = null;

        public LogicControl(Ballz game)
        {
            Game = game;
            state = GameState.Unknown;
            ButtonRepeat = new Timer(62.5);
            ButtonRepeat.AutoReset = true;
            repeatHandler = (s,e)=>{};
            ButtonRepeat.Elapsed += repeatHandler;
            ButtonDelay = new Timer(1000);
            ButtonDelay.AutoReset = false;
            ButtonDelay.Elapsed += (s, e) =>
            {
                ButtonRepeat.Stop();
                ButtonRepeat.Start();
            };
        }

        public void SetMainMenu(MenuPanel menu)
        {
            activeMenu.Clear();
            activeMenu.Push(menu); //TODO: uncast
            state = GameState.MenuState;
            menu.OnOpen();
            Ballz.The().LockMouse = false;
        }

        public void OpenMenu(MenuPanel newMenu)
        {
            var oldMenu = activeMenu.Peek();
            oldMenu.OnClose();
            newMenu.OnOpen();

            activeMenu.Push(newMenu);
            RaiseMessageEvent(new MenuMessage(activeMenu.Peek()));
            state = GameState.MenuState;
            Ballz.The().LockMouse = false;
        }
        
        public void MenuGoBack()
        {
            var oldMenu = activeMenu.Pop();
            oldMenu.OnClose();
            var newMenu = activeMenu.Peek();
            newMenu.OnOpen();
        }

        public void StartGame(MatchSettings settings, bool isMultiplayer = false, int localPlayerId = -1)
        {
            ButtonRepeat.Stop();
            ButtonDelay.Stop();
            // Go back to main menu so it will show when the user enters the menu later
            MenuGoBack();
            // Select the "Continue" entry
            activeMenu.Peek().SelectIndex(0);

            state = GameState.SimulationState;
            Ballz.The().LockMouse = true;
            if (Game.Match != null)
                Game.Match.Dispose();

            Game.Match = settings.GameMode.StartSession(Game, settings, isMultiplayer, localPlayerId);
            Game.Match.Start();
            RaiseMessageEvent(new LogicMessage(LogicMessage.MessageType.GameMessage));
        }

        public void ContinueGame()
        {
            ButtonRepeat.Stop();
            ButtonDelay.Stop();
            state = GameState.SimulationState;
            RaiseMessageEvent(new LogicMessage(LogicMessage.MessageType.GameMessage));
        }
        
        public event EventHandler<Message> Message;

        protected virtual void RaiseMessageEvent(Message msg)
        {
            Message?.Invoke(this, msg);
            if(state == GameState.MenuState)
                Ballz.The().Services.GetService<SoundControl>().StartMusic(SoundControl.MenuMusic);
            else
                Ballz.The().Services.GetService<SoundControl>().StopMusic(SoundControl.MenuMusic);
        }

        public void HandleNetworkMessage(object sender, Message message)
        {

        }

        public void HandleInputMessage(object sender, Message message)
        {
            if (message.Kind != Messages.Message.MessageType.InputMessage)
                return;

            if (((InputMessage)message).Kind == InputMessage.MessageType.ControlsConsole && ((InputMessage)message).Pressed)
                RaiseMessageEvent(new LogicMessage(LogicMessage.MessageType.PerformanceMessage));

            switch (state)
            {
                case GameState.MenuState:
                    MenuLogic((InputMessage)message);
                    break;
                case GameState.SimulationState:
                    GameLogic((InputMessage)message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CheckInputMode((InputTranslator)sender);
        }

        private void GameLogic(InputMessage msg)
        {
            if (msg.Pressed)
            {
                switch (msg.Kind)
                {
                    case InputMessage.MessageType.ControlsBack:
                        state = GameState.MenuState;
                        RaiseMessageEvent(new LogicMessage(LogicMessage.MessageType.GameMessage));
                        //todo: implement LogicMessage and use it here
                        break;
                    case InputMessage.MessageType.ControlsUp:
                        break;
                    case InputMessage.MessageType.ControlsDown:
                        break;
                    case InputMessage.MessageType.ControlsLeft:
                        break;
                    case InputMessage.MessageType.ControlsRight:
                        break;
                    case InputMessage.MessageType.ControlsAction:
                        break;
                    case InputMessage.MessageType.RawInput:
                        break;
                    default:
                        //throw new ArgumentOutOfRangeException();
                        break;
                }
            }
        }

        private void MenuLogic(InputMessage msg)
        {
            MenuPanel top = activeMenu.Peek();
            //KeyPress Events
            if (msg.Kind == InputMessage.MessageType.RawInput || msg.Kind == InputMessage.MessageType.RawBack || msg.Pressed)
            {
                ButtonDelay.Stop();
                ButtonDelay.Start();
                ButtonRepeat.Elapsed -= repeatHandler;
                repeatHandler = (s, e) => this.MenuLogic(msg);
                ButtonRepeat.Elapsed += repeatHandler;

                switch (msg.Kind)
                {
                    case InputMessage.MessageType.ControlsAction:
                        //(top.SelectedItem as MenuButton)?.DoClick();
                        //Ballz.The().Services.GetService<SoundControl>().PlaySound(SoundControl.AcceptSound);
                        break;
                    case InputMessage.MessageType.ControlsBack:
                        Ballz.The().Services.GetService<SoundControl>().PlaySound(SoundControl.DeclineSound);
                        if (activeMenu.Count == 1) // exit if we are in main menuToPrepare
                            Ballz.The().Exit();     //TODO: this is rather ugly find a nice way to terminate the programm like sending a termination message
                        else
                        {
                            if (rawInput)
                                rawInput = false;
                            else
                            {
                                MenuGoBack();
                            }
                        }

                        RaiseMessageEvent(new MenuMessage(activeMenu.Peek()));
                        break;
                    case InputMessage.MessageType.ControlsUp:
                        if (top.SelectedItem != null)
                        {
                            Ballz.The().Services.GetService<SoundControl>().PlaySound(SoundControl.SelectSound);
                            top.SelectPrevious();
                            RaiseMessageEvent(new MenuMessage(top));
                        }

                        break;
                    case InputMessage.MessageType.ControlsDown:
                        if (top.SelectedItem != null)
                        {
                            Ballz.The().Services.GetService<SoundControl>().PlaySound(SoundControl.SelectSound);
                            top.SelectNext();
                            RaiseMessageEvent(new MenuMessage(top));
                        }

                        break;
                    case InputMessage.MessageType.ControlsLeft:                                     
                        (top.SelectedItem as IChooseable)?.SelectPrevious();
                        break;
                    case InputMessage.MessageType.ControlsRight:
                        (top.SelectedItem as IChooseable)?.SelectNext();                      
                        break;
                    case InputMessage.MessageType.RawInput:                        
                            (top.SelectedItem as IRawInputConsumer)?.HandleRawKey(msg.Key);
                        break;
                    case InputMessage.MessageType.RawBack:
                        (top.SelectedItem as IRawInputConsumer)?.HandleBackspace();
                        break;
                    default:
                        //throw new ArgumentOutOfRangeException();
                        break;
                }
            }
            else
            {
                //Key release events
                if (!msg.Pressed)
                {
                    ButtonRepeat.Stop();
                    ButtonDelay.Stop();
                    switch(msg.Kind)
                    {
                        case InputMessage.MessageType.ControlsLeft:
                            break;
                        case InputMessage.MessageType.ControlsRight:
                            break;
                    }
                }
            }
        }

        public void Update(Microsoft.Xna.Framework.GameTime time)
        {
            if (activeMenu.Count > 0)
                activeMenu.Peek().Update();
        }

        /// <summary>
        /// Checks the input mode.
        /// TODO: refactor the Menu logic to a menuLogic class or use a partial class definition as this file seems to become messy
        /// </summary>
        void CheckInputMode(InputTranslator translator)
        {
            if (rawInput)
            {
                translator.Mode = InputTranslator.InputMode.RAW;
                ButtonRepeat.Stop();
                ButtonDelay.Stop();
            }
        }

        private enum GameState
        {
            Unknown,
            MenuState,
            SimulationState
        }
    }
}