using System;

namespace Photon.Server.Internal
{
    internal class ViewModelBase
    {
        public string PageTitle {get; set;}
        public string CopyrightYear {get; set;}


        public virtual void Build()
        {
            PageTitle = "Photon Server";
            CopyrightYear = DateTime.Now.Year.ToString();
        }
    }
}
