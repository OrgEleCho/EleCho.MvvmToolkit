using EleCho.MvvmToolkit.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTestApp
{
    internal class SomeBaseClass
    {
    }

    [ObservableObject]
    internal partial class SomeModel : SomeBaseClass
    {
        [ObservableProperty]
        private string _name = string.Empty;
    }
}
