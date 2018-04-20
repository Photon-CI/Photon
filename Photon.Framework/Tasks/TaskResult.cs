using Photon.Framework.Extensions;
using System;

namespace Photon.Framework.Tasks
{
    [Serializable]
    public class TaskResult
    {
        public bool Successful {get; set;}
        public bool Cancelled {get; set;}
        public string Message {get; set;}


        public static TaskResult Ok(string message = "Ok")
        {
            return new TaskResult {
                Successful = true,
                Cancelled = false,
                Message = message,
            };
        }

        public static TaskResult Cancel(string message = "Cancelled.")
        {
            return new TaskResult {
                Successful = false,
                Cancelled = true,
                Message = message,
            };
        }

        public static TaskResult Error(string message = "Error!")
        {
            return new TaskResult {
                Successful = false,
                Cancelled = false,
                Message = message,
            };
        }

        public static TaskResult Error(Exception error)
        {
            return new TaskResult {
                Successful = false,
                Cancelled = false,
                Message = error?.UnfoldMessages(),
            };
        }
    }
}
