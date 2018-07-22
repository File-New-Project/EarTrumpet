using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class ContextMenuItem
    {
        public string DisplayName;
        public ICommand InvokeAction;
        public bool IsChecked;
        public ContextMenuItem[] Children;

        public ContextMenuItem(string displayName, ICommand cmd)
        {
            InvokeAction = cmd;
            DisplayName = displayName;
        }

        public ContextMenuItem(string displayName, IEnumerable<ContextMenuItem> children)
        {
            DisplayName = displayName;
            Children = children.ToArray();
        }
    }
}