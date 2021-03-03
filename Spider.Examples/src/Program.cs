using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spider.ArcheType;
using Spider.WebAPI.Abstraction;
using Spider.WebAPI.Annotations;
using Spider.WebAPI.Renders;

namespace Spider.Examples
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            await Spider.WebAPI.Builder.BuildAsync(new string[] { "http://127.0.0.1:8082/" }, new System.Threading.CancellationToken());
        }
    }

    [Controller]
    public class HomeController
    {
        [AutoWire]
        public IContext Context { get; set;}
        [Verb("GET")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(15000);
            await Context.WebSocket.AcceptAsync(null, 1024, TimeSpan.FromSeconds(15));
            Context.WebSocket.Message += (sender, e) => {
                Console.WriteLine(e.ReadMessage(Encoding.UTF8));
                e.RespondAsync("I Recieved It!", Encoding.UTF8, cts.Token);
                Console.WriteLine("Done");
            };
            await Context.WebSocket.PushAsync("Hello", System.Text.Encoding.Default, cts.Token);
            await Context.WebSocket.RecieveAsync(1024, cts.Token);
            return null;
        }
    }
}