// Copyright 2025 Robert Adams
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace org.herbal3d.Sage {

    public partial class SageMain {

        public static IHost SageHost { get; private set; } = default!;

        /// <summary>
        /// Gets an instance of <typeparamref name="T"/>
        /// Will throw if its unable to provide a <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public static T GetService<T>() where T : notnull {
            return SageHost.Services.GetRequiredService<T>()
                ?? throw new ApplicationException($"There requested type {typeof(T).FullName} could not be provided.");
        }

        public static async Task Main(string[] args) {

            SageHost = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) => {
                    // CreateDefaultBuilder already adds 'appsettings.json',
                    //     'appsettings.Development.json', and environment variables.

                    // Add the default logging config first so other configs can override it.
                    config.AddInMemoryCollection(LoggingConfig.DefaultValues);
                    config.AddJsonFile("Sage.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables("Sage_");
                    // re-add command line args so they override other settings
                    config.AddCommandLine(Environment.GetCommandLineArgs());
                })
                .ConfigureLogging(logging => {
                    // TODO: understand how logging configuration works with CreateDefaultBuilder
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    // Additional logging configuration if needed
                })
                .ConfigureServices((context, services) => {
                    // Configuration binding
                    RegisterConfigurationOptions(context, services);
                    // Core Services
                    RegisterServices(context, services);
                    // Infrastructure Services
                    //     e.g., database, messaging, etc.
                    // Application Services
                    //        e.g., jobrunners, etc.
                    // Background Workers
                    //    e.g., hosted services, etc.
                    RegisterWorkers(context, services);


                })
                .Build();

            LogStartup(SageHost.Services.GetRequiredService<ILogger<SageMain>>());

            await SageHost.RunAsync();
        }

        [LoggerMessage(0, LogLevel.Information, "Sage application starting up.")]
        public static partial void LogStartup(ILogger logger);
        [LoggerMessage(0, LogLevel.Information, "Sage application configuration complete.")]
        public static partial void LogConfigurationComplete(ILogger logger);


        private static void RegisterConfigurationOptions(HostBuilderContext pContext, IServiceCollection pServices) {
            pServices.AddOptions<MyOptions>()
                        .Bind(pContext.Configuration.GetSection("MyOptions"));
        }

        private static void RegisterServices(HostBuilderContext pContext, IServiceCollection pServices) {
            // lots of _serviceCollection.AddSingleton<IInterface, InterfaceClass>();
            // Register services here if needed

            pServices.AddSingleton<IMyService, MyService>();
        }

        private static void RegisterWorkers(HostBuilderContext pContext, IServiceCollection pServices) {
            // lots of _serviceCollection.AddHostedService<WorkerClass>();
            // Register background workers here if needed
            pServices.AddHostedService<BackgroundWorker>();
        }
    }
}
