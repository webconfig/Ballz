﻿//
//  CheckBox.cs
//
//  Author:
//       Martin <Martin.Schultz@RWTH-Aachen.de>
//
//  Copyright (c) 2015 SPAG
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;

namespace Ballz.Menu
{
    public class CheckBox : Leaf
    {
        Settings.BooleanSetting Value;
        public CheckBox(string name, Settings.BooleanSetting value, bool selectable = true ) : base(name, selectable)
        {
            Value = value;
            OnSelect += () => Value.Value = !Value.Value;
        }

        public override void HandleBackspace()
        {            
        }

        public override void HandleRawKey(char key)
        {          
        }

        public override string DisplayName => Name + Value.Value;
    }
}

