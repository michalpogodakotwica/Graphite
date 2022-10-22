using System;

namespace Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowInNodeAttribute : Attribute
    {
        public string LabelText = "";
        public bool HideLabel = false;
        public bool IsReadOnly = false;
    }
}