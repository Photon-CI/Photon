using System;
using System.Collections.Generic;

namespace Photon.Library
{
    public class ViewModelBase
    {
        public string PageTitle {get; set;}
        public string CopyrightYear {get; set;}
        public bool SecurityEnabled {get; set;}
        public List<Exception> Errors {get;}


        public ViewModelBase()
        {
            PageTitle = "Photon";
            CopyrightYear = DateTime.Now.Year.ToString();
            Errors = new List<Exception>();
        }
    }
}
