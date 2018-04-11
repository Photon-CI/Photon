using System;

namespace Photon.Framework.Server
{
    [Serializable]
    public class ScriptResult
    {
        public bool Successful {get; set;}
        public bool Cancelled {get; set;}
        public string Message {get; set;}


        public static ScriptResult Ok(string message = "Ok")
        {
            return new ScriptResult {
                Successful = true,
                Cancelled = false,
                Message = message,
            };
        }

        public static ScriptResult Cancel(string message = "Cancelled.")
        {
            return new ScriptResult {
                Successful = false,
                Cancelled = true,
                Message = message,
            };
        }

        public static ScriptResult Error(string message = "Error!")
        {
            return new ScriptResult {
                Successful = false,
                Cancelled = false,
                Message = message,
            };
        }

        public static ScriptResult Error(Exception error)
        {
            return new ScriptResult {
                Successful = false,
                Cancelled = false,
                Message = error?.Message ?? "Error!",
            };
        }
    }
}
