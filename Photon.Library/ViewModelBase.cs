using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photon.Library
{
    public abstract class ViewModelBase
    {
        public string PageTitle {get; set;}
        public string CopyrightYear {get; set;}
        public bool SecurityEnabled {get; set;}
        public List<Exception> Errors {get;}


        protected ViewModelBase()
        {
            PageTitle = "Photon";
            CopyrightYear = DateTime.Now.Year.ToString();
            Errors = new List<Exception>();
        }

        protected abstract void OnBuild();

        public void Build()
        {
            OnBuild();
        }
    }

    public abstract class ViewModelAsyncBase
    {
        public string PageTitle {get; set;}
        public string CopyrightYear {get; set;}
        public bool SecurityEnabled {get; set;}
        public List<Exception> Errors {get;}


        protected ViewModelAsyncBase()
        {
            PageTitle = "Photon";
            CopyrightYear = DateTime.Now.Year.ToString();
            Errors = new List<Exception>();
        }

        protected abstract Task OnBuildAsync();

        public async Task<bool> BuildAsync()
        {
            try {
                await OnBuildAsync();
                return true;
            }
            catch (Exception error) {
                Errors.Add(error);
                return false;
            }
        }
    }
}
