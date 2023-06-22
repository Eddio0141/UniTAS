using System.Threading;

namespace UniTAS.Patcher.Services;

public interface IThreadTracker
{
    void ThreadStart(Thread thread);
}