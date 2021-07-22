using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GmodNET.API;
using System.Collections.Concurrent;

namespace TemplateModuleWeb
{
    public class GmodInteropService
    {
        bool disposed;
        string onTickCallbackId;
        ConcurrentQueue<TaskCompletionSource<List<string>>> getPlayersTasks;

        public GmodInteropService(ILua lua)
        {
            disposed = false;
            onTickCallbackId = Guid.NewGuid().ToString();
            getPlayersTasks = new ConcurrentQueue<TaskCompletionSource<List<string>>>();

            lua.PushGlobalTable();
            lua.GetField(-1, "hook");
            lua.GetField(-1, "Add");
            lua.PushString("Tick");
            lua.PushString(onTickCallbackId);
            lua.PushManagedFunction(lua =>
            {
                SetPlayerList(lua);
                return 0;
            });
            lua.MCall(3, 0);
            lua.Pop(2);
        }

        public void Dispose(ILua lua)
        {
            if(!disposed)
            {
                disposed = true;

                lua.PushGlobalTable();
                lua.GetField(-1, "hook");
                lua.GetField(-1, "Remove");
                lua.PushString("Tick");
                lua.PushString(onTickCallbackId);
                lua.MCall(2, 0);
                lua.Pop(2);
            }
        }

        public Task<List<string>> GetPlayersList()
        {
            TaskCompletionSource<List<string>> getPlayersTask = new TaskCompletionSource<List<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
            getPlayersTasks.Enqueue(getPlayersTask);
            return getPlayersTask.Task;
        }

        void SetPlayerList(ILua lua)
        {
            if(!getPlayersTasks.IsEmpty)
            {
                List<string> players = new List<string>();
                Exception executionException = null;

                try
                {
                    lua.PushGlobalTable();
                    lua.GetField(-1, "player");
                    lua.GetField(-1, "GetAll");
                    lua.MCall(0, 1);
                    lua.PushNil();
                    while (lua.Next(-2) != 0)
                    {
                        lua.GetField(-1, "Nick");
                        lua.Push(-2);
                        lua.MCall(1, 1);
                        players.Add(lua.GetString(-1));
                        lua.Pop(2);
                    }
                    lua.Pop(lua.Top());
                }
                catch (Exception e)
                {
                    executionException = e;
                }

                TaskCompletionSource<List<string>> task;
                while(getPlayersTasks.TryDequeue(out task))
                {
                    if (executionException != null)
                    {
                        task.SetResult(players);
                    }
                    else
                    {
                        task.SetException(executionException);
                    }
                }
            }
        }
    }
}
