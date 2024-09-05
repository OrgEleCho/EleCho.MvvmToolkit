using EleCho.MvvmToolkit.ComponentModel;

namespace TestApp;

class SomeBaseClass
{ 
}

[ObservableObject]
partial class TestModel : SomeBaseClass
{
    [ObservableProperty]
    private string _id = string.Empty;
}