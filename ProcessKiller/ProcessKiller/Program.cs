using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ProcessKiller
{
    class Program
    {
        static int port = 0;
        static IWebHost webhost;
        static async Task Main(string[] args)
        {
            string input = "";

            //input webhost port
            while (port == 0)
            {
                Console.WriteLine("\n");
                Console.WriteLine("setting webhost port");
                Console.WriteLine(" (the port should be allowed in the firewall first. )");
                Console.Write("Input port:");
                input = Console.ReadLine();
                if (!Int32.TryParse(input, out port)) 
                {
                    Console.WriteLine("\n");
                    Console.Write("slease input number");
                    Console.WriteLine("\n");
                }
            }

            //webhost starting
            Console.WriteLine("\n");
            Console.WriteLine("system starting");
            Console.WriteLine("\n");
            Console.WriteLine($"the webhost route is http://127.0.0.1:{port}");
            Console.WriteLine("\n");

            webhost = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{port}")
                .Configure(configure)
                .Build();
            webhost.RunAsync(); //don't await to lock main thread.

            var openurl = new System.Diagnostics.Process();
            try
            {
                openurl.StartInfo.UseShellExecute = true;
                openurl.StartInfo.FileName = $"http://127.0.0.1:{port}";
                openurl.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR:{ex.Message}");
            }

            //webhost started
            Console.WriteLine("\n");
            Console.WriteLine("system started");
            Console.WriteLine("\n");


            //wait for user command
            while ("Q" != input.ToUpper())
            {
                Console.WriteLine("\nPress q to close system\n\n");
                input=Console.ReadKey().KeyChar.ToString();
            }

            //system closing
            Console.WriteLine("\n");
            Console.WriteLine("system closing");
            Console.WriteLine("\n");
            await webhost.StopAsync();

            //system closed
            Console.WriteLine("\n");
            Console.WriteLine("system closed");
            Console.WriteLine("\n");
            Console.WriteLine("Press anykey to exit\n\n");
            Console.WriteLine("\n");
            Console.ReadKey();
        }

        protected static void configure(IApplicationBuilder app)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.Run(async (context) => {
                var request = context.Request;
                var response = context.Response;
                var path = request.Path.Value;

                try
                {
                    switch (path)
                    {
                        default:
                            response.StatusCode = 404;
                            await response.WriteAsync("Wrong path");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = 500;
                    await response.WriteAsync(ex.Message);
                }
            });
        }
    }
}
