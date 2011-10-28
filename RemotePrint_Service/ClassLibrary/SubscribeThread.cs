using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Redis;
using System.Diagnostics;
using System.Threading;
namespace ClassLibrary
{
    public class SubscribeThread : IDisposable
    {
        string _redis = "";
        string _key = "";
        string lastmessage = "";
        public delegate void GotMessageDeleg(string message);
        public static event GotMessageDeleg GotMessage;
        public bool ok = true;
        public SubscribeThread(string redis, GotMessageDeleg msg, string key)
        {
            _redis = redis;
            _key = key;
            GotMessage += msg;

        }

        public void Start()
        {


            try
            {
                do
                {
                    try
                    {
                        using (var redisConsumer = new RedisClient(_redis))
                        {
                            //redisConsumer.Db = 2;
                            //redisConsumer.SetEntry(_key, "ON");
                            using (var subscription = redisConsumer.CreateSubscription())
                            {
                                subscription.OnMessage = (channel, msg) =>
                                {
                                    Console.WriteLine("Received '{0}' from channel '{1}'", msg, channel);

                                    if (msg == "bye bye")
                                    {
                                        //EventLog.WriteEntry("Splickit Remote Print", "Got bye bye");
                                        ok = false;
                                        lock (this)
                                        {
                                            GotMessage(msg);
                                            // Invoke(GotMessage(msg), true);

                                        }

                                    }
                                    else
                                        lock (this)
                                        {
                                            GotMessage(msg);
                                            // Invoke(GotMessage(msg), true);

                                        }
                                };
                                lastmessage = "";
                                if (ok)
                                {

                                    subscription.SubscribeToChannels(_key); //blocking
                                    lastmessage = "";
                                }

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        //no redis
                        //reconnect?
                        //if (ex.Message != lastmessage)
                       //     EventLog.WriteEntry("SplickIt Remote Printing", "Subscriber Error:" + ex.Message);
                        lastmessage = ex.Message;
                        if (ok)
                        {
                            lock (this)
                            {
                                GotMessage("Error:" + ex.Message);
                                // Invoke(GotMessage(msg), true);

                            }
                            System.Threading.Thread.Sleep(10000);
                        }

                    }
                } while (ok);
            }
            catch (ThreadInterruptedException ex)
            {
                /* Clean up. */
                //if (ex.Message != lastmessage)
                  //  EventLog.WriteEntry("SplickIt Remote Printing", "Subscriber Error:" + ex.Message);
                lastmessage = ex.Message;
                GotMessage("Error:" + ex.Message);
            }
        }
        public void Dispose()
        {


        }
    }
}
