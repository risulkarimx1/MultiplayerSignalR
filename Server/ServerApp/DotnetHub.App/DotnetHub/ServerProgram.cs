using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;

namespace DotnetHub
{
    class ServerProgram
    {
        public static bool IsLocked { set; get; }

        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:8080/"))
            {
                Console.WriteLine("Server running at http://localhost:8080/");
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR("/boards", new HubConfiguration());
        }
    }

    public class BoardHub : Hub
    {
        public BoardHub()
        {
            Console.WriteLine();
        }

        public async Task TryLock()
        {
            if (ServerProgram.IsLocked == false)
            {
                ServerProgram.IsLocked = true;
                await Clients.Caller.SendLockStatuesFromServer(true);
            }
            else
            {
                await Clients.Caller.SendLockStatuesFromServer(false);
            }
        }

        public async Task ReleaseLockToServer()
        {
            ServerProgram.IsLocked = false;
            await Clients.Caller.SendLockStatuesFromServer(false);
        }

        public async Task SetPositionToServer(float x, float y, float z)
        {
            await Clients.Others.SendPositionFromServer(x, y, z);
        }
    }
}