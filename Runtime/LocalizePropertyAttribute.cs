using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIDocumentLocalization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class LocalizeProperty : Attribute { }
}
