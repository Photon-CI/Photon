namespace Photon.Framework.Tasks
{
    public interface ITask
    {
        TaskResult Run(TaskContext context);
    }
}
