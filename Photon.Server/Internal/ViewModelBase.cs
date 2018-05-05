using System;
using System.Collections.Generic;

namespace Photon.Server.Internal
{
    internal class ViewModelBase
    {
        public string PageTitle {get; set;}
        public string CopyrightYear {get; set;}
        public List<Exception> Errors {get;}


        public ViewModelBase()
        {
            PageTitle = "Photon Server";
            CopyrightYear = DateTime.Now.Year.ToString();
            Errors = new List<Exception>();
        }

        public virtual void Build() {}
    }
}
