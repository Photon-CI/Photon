using Photon.CLI.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photon.CLI.Commands
{
    public class DeployCommand : ICommand
    {
        public string ProjectName {get; set;}
        public string ProjectVersion {get; set;}


        public void Run()
        {
            //
        }
    }
}
