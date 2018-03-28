using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photon.Framework.Domain
{
    //[Serializable]
    public class DomainAgentTask : MarshalByRefObject
    {
        private readonly TaskCompletionSource<object> completeEvent;


        public DomainAgentTask(string scriptName)
        {
            completeEvent = new TaskCompletionSource<object>();
        }

        public async Task<object> Run()
        {

            return await completeEvent.Task;
        }
    }
}
