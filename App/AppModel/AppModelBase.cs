using Ion.Input;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ion.Core;

public abstract record class AppModelBase : Model
{
    public virtual string GitHub => $@"https://github.com/ionsharp";

    public virtual string GitHubProject => GitHub + $@"/{XAssembly.GetInfo(AssemblySource.Entry).Product}";

    public ICommand HelpCommand
        => Commands[nameof(HelpCommand)]
        ??= new RelayCommand(() => Try.Do(() => System.Diagnostics.Process.Start(new ProcessStartInfo { FileName = GitHubProject + "/wiki", UseShellExecute = true })), () => true);
}