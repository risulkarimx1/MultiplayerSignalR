using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;

namespace DotnetHub
{
    class Program
    {
        static void Main(string[] args)
        {
            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 or http://+:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.

            LockDataContext.Initialize();
            using (WebApp.Start<Startup>("http://localhost:8080/"))
            {
                Console.WriteLine("Server running at http://localhost:8080/");
                Console.ReadLine();
            }
        }
    }

    public class LockModel
    {
        public bool IsLocked { get; set; }
    }
    
    public static class LockDataContext
    {
        private static LockModel _lockModel;
        static string filePath = Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName, "LockState.json"); 
        public static void Initialize()
        {
            _lockModel = new LockModel() { IsLocked = false };
            if (File.Exists(filePath) == false)
            {
                File.Create(filePath);
            }

            File.WriteAllText(filePath, JsonConvert.SerializeObject(_lockModel));
        }
        

        public static void SetLockState(bool state)
        {
            _lockModel.IsLocked = state;
            File.WriteAllText(filePath,JsonConvert.SerializeObject(_lockModel));
        }

        public static bool IsLocked()
        {
            using (StreamReader file = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                LockModel lockModel = (LockModel)serializer.Deserialize(file, typeof(LockModel));
                return lockModel.IsLocked;
            }
        }
    }
    
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR("/chatHub", new HubConfiguration());
        }
    }

    public class ChatHub: Hub
    {
        public ChatHub()
        {
            Console.WriteLine();
        }

        public async Task TryLock()
        {
            var isLocked = LockDataContext.IsLocked();
            if (isLocked == false)
            {
                LockDataContext.SetLockState(true);
                await Clients.Caller.SendLockStatus(true);
            }
            else
            {
                await Clients.Caller.SendLockStatus(false);
            }
        }

        public async Task ReleaseLock()
        {
            LockDataContext.SetLockState(false);
            await Clients.Caller.SendLockStatus(false);
        }

        public async Task SendMessage(string name, string message)
        {
            Console.WriteLine($"Message came {name} - {message}");
            await Clients.All.SendMessageToClient(name, message);
        }

        public async Task UpdatePosition(float x, float y, float z)
        {
            await Clients.Others.SendUpdatedPosition(x, y, z);
        }
    }
}