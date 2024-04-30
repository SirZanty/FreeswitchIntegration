using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FreeswitchIntegration;
using Microsoft.Extensions.Logging;
using NEventSocket;
using NEventSocket.FreeSwitch;
using Newtonsoft.Json;

NEventSocket.Logging.Logger.Configure(new LoggerFactory());

string codigoError = "";

using (var listener = new OutboundListener(8084))
{
    listener.Connections.Subscribe(
      async socket => {
          await socket.Connect();

          var uuid = socket.ChannelData.Headers[HeaderNames.UniqueId];
          Console.WriteLine("OutboundSocket connected for channel " + uuid);

          await socket.SubscribeEvents(EventName.ChannelHangup);
          await socket.SubscribeEvents(EventName.All);

          socket.ChannelEvents
              .Where(x => x.EventName == EventName.ChannelHangup && x.UUID == uuid)
              .Take(1)
              .Subscribe(async x => {
                  Console.WriteLine("Hangup Detected on " + JsonConvert.SerializeObject(x));
                  Console.WriteLine("Hangup Detected on " + x.HangupCause);
                  codigoError += x.HangupCause;
                  await socket.Exit();
              });


          
          socket.ChannelEvents
             .Subscribe( x => {
                 Console.WriteLine("Events: " + x.EventName);
             });

          




          await socket.Filter(HeaderNames.UniqueId, uuid);

          await socket.Linger();

          Console.WriteLine("var:" + socket.ChannelData.GetVariable("holagp"));

          await socket.ExecuteApplication(uuid, "answer");

          PlayGetDigitsOptions playDigitsOptions = new PlayGetDigitsOptions();
          playDigitsOptions.PromptAudioFile = "/tmp/0ec62ba9-ceb7-4f70-bb8e-6d1adc70add8.mp3";


          var digit = await socket.PlayGetDigits(uuid, playDigitsOptions);
          Console.WriteLine("digit:" + JsonConvert.SerializeObject(digit));
          if (digit.Digits=="1")
          {
              await socket.Play(uuid,"/tmp/1.mp3");
          }
          if (digit.Digits == "0")
          {
              await socket.Play(uuid, "/tmp/0.mp3");
          }
          await socket.Hangup(uuid, HangupCause.NormalClearing);
          codigoError += "16final";
      });

    listener.Start();

    await Util.WaitForEnterKeyPress();


    Console.WriteLine(codigoError);
}