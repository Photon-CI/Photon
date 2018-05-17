﻿using Photon.Framework.Server;
using Photon.Library;
using Photon.Server.Internal;
using System.Linq;

namespace Photon.Server.ViewModels.Agents
{
    internal class AgentIndexVM : ViewModelBase
    {
        public ServerAgent[] Agents {get; set;}


        public void Build()
        {
            Agents = PhotonServer.Instance.Agents.All.ToArray();
        }
    }
}
