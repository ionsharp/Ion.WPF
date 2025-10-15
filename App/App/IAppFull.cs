using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ion.Core;

public interface IAppFull : IApp
{
    IAppModel Model { get; }
}