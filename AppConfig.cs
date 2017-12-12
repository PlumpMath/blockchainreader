using Microsoft.Extensions.Configuration;
using System.IO;
using System;

namespace blockchain_parser
{
    public sealed class AppConfig
    {
        private static IConfigurationRoot Configuration { get; set; }
        static AppConfig() {
             var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public static string getConnectionString(string connection_string) {
            return Configuration["ConnectionStrings:"+connection_string];
        }

        public static string WebsocketNodeUrl {
            get {
                return Configuration["Blockchain:WebsocketNodeUrl"];
            }
        }

        public static string RpcNodeUrl {
            get {
                return Configuration["Blockchain:RpcNodeUrl"];
            }
        }

        public static bool WebSocketLogVerbose {
              get {
                return Convert.ToBoolean(Configuration["WebSocketLogVerbose"]);
            }
        }

        public static ulong WebSocketMessageLimit {
            get {
                return Convert.ToUInt64(Configuration["WebSocketMessageLimit"]);
            }
        }

        public static uint MaxTransactionPerSqlQuery {
            get {
                return Convert.ToUInt32(Configuration["MaxTransactionPerSqlQuery"]);
            }
        }

         public static uint ReceiveChunkSize {
            get {
                return Convert.ToUInt32(Configuration["ReceiveChunkSize"]);
            }
        }

        public static string SmtpHost {
            get {
                return Configuration["SMTP:Host"];
            }
        }

         public static ushort SmtpPort {
            get {
                return Convert.ToUInt16(Configuration["SMTP:Port"]);
            }
        }

        public static string SmtpUsername {
            get {
                return Configuration["SMTP:Username"];
            }
        }

        public static string SmtpPassword {
            get {
                return Configuration["SMTP:Password"];
            }
        }

        public static Tuple<string, string> SmtpFrom {
            get {
                return new Tuple<string, string>(Configuration["SMTP:FromAddress"], Configuration["SMTP:FromName"]);
            }
        }

        public static bool SmtpEncryption {
            get {
                return Convert.ToBoolean(Configuration["SMTP:Encryption"]);
            }
        }
    }
}