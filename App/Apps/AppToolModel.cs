using Ion.Core;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ion.Core;

public abstract record class AppToolModel : AppModelBase
{
    public override string GitHubProject => GitHub + $@"/Ion.Tools.{XAssembly.GetInfo(AssemblySource.Entry).Product}";
}
