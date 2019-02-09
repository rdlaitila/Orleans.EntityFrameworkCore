using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.EntityFrameworkCore.Extensions;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;

namespace Orleans.EntityFrameworkCore.Tests
{
    public static class TestUtil
    {
        public static IClusterClient BuildClient(
            Action<IServiceCollection> configureServicesCallback
        )
        {
            return new ClientBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "ClusterId1";
                    options.ServiceId = "ServiceId";
                })
                .ConfigureServices(services =>
                {
                    configureServicesCallback.Invoke(services);
                })
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(TestSuite).Assembly).WithReferences()
                )
                .ConfigureLogging(options =>
                {
                    options.SetMinimumLevel(LogLevel.Warning);
                    options.AddConsole();
                })
                .UseEntityFrameworkClustering()
                .Build();
        }

        public static ISiloHost BuildSiloHost(
            int siloPort,
            int gatewayPort,
            Action<IServiceCollection> configureServicesCallback
        )
        {
            return new SiloHostBuilder()
                .AddEntityFrameworkStorage()
                .ConfigureEndpoints(IPAddress.Loopback, siloPort, gatewayPort)
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "ClusterId1";
                    options.ServiceId = "ServiceId";
                })
                .ConfigureServices(services =>
                {
                    configureServicesCallback.Invoke(services);
                })
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(TestSuite).Assembly).WithReferences()
                )
                .ConfigureLogging(options =>
                {
                    options.SetMinimumLevel(LogLevel.Warning);
                    options.AddConsole();
                })
                .UseEntityFrameworkClustering()
                .UseEntityFrameworkReminders()
                .Build();
        }

        public static async Task WaitForDockerLogOutput(string containerName, string lineContains, DateTime timeoutAfter)
        {
            Console.WriteLine($"waiting for docker log output: '{lineContains}'");

            while (true)
            {
                if (DateTime.UtcNow > timeoutAfter)
                    throw new TimeoutException(
                        $"timeout waiting for docker log output: '{lineContains}'"
                    );

                using (var proc = new Process())
                {
                    var output = new List<string>();

                    proc.StartInfo.FileName = "docker";
                    proc.StartInfo.Arguments = $"logs {containerName}";
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null && !string.IsNullOrWhiteSpace(args.Data))
                            output.Add(args.Data);
                    };
                    proc.ErrorDataReceived += (sender, args) =>
                    {
                        if (args.Data != null && !string.IsNullOrWhiteSpace(args.Data))
                            output.Add(args.Data);
                    };
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    proc.WaitForExit();

                    var stdoutContainsLine = output
                        .Any(line => line.Contains(lineContains));

                    if (stdoutContainsLine)
                    {
                        Console.WriteLine($"found docker log output: '{lineContains}'");
                        return;
                    }
                }

                Console.WriteLine($"no docker log line found. waiting 1 second");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public static async Task WaitForTcpSocket(string host, int port, DateTime timeoutAfter)
        {
            while (true)
            {
                if (DateTime.UtcNow > timeoutAfter)
                    throw new Exception(
                        "timeout exceeded waiting for socket to establish"
                    );

                Console.WriteLine(
                    $"waiting for socket. " +
                    $"host={host} " +
                    $"port={port} " +
                    $"timeoutAfter={timeoutAfter}"
                );

                var socket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );

                using (socket)
                {
                    try
                    {
                        await socket.ConnectAsync(host, port);

                        Console.WriteLine(
                            $"successfully connected socket. " +
                            $"host={host} " +
                            $"port={port}"
                        );

                        return;
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine(
                            $"error connecting socket. " +
                            $"host={host} " +
                            $"port={port}. " +
                            $"error={e}"
                        );

                        await Task.Delay(
                            TimeSpan.FromSeconds(1)
                        );
                    }
                }
            }
        }

        public class DockerContainer : IDisposable
        {
            private readonly string _containerName;

            private readonly string _containerImage;

            private readonly string[] _containerArgs;

            private readonly string _containerCmd;

            public DockerContainer(
                string name,
                string image,
                string[] args,
                string cmd = ""
            )
            {
                _containerImage = image;
                _containerName = name;
                _containerArgs = args;
                _containerCmd = cmd;
            }

            public void Start()
            {
                Stop();
                Process.Start("docker", $"run -d --name {_containerName} {string.Join(" ", _containerArgs)} {_containerImage} {_containerCmd}")?.WaitForExit();
            }

            public void Stop()
            {
                Process.Start("docker", $"rm -v -f {_containerName}")?.WaitForExit();
            }

            public void Dispose()
            {
                Stop();
            }
        }
    }
}