using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FreeswitchIntegration;
using Microsoft.Extensions.Logging;
using NEventSocket;
using NEventSocket.FreeSwitch;

NEventSocket.Logging.Logger.Configure(new LoggerFactory());

using (var socket = await InboundSocket.Connect("localhost", 8021, "ClueCon"))
{
    var apiResponse = await socket.SendApi("status");
    Console.WriteLine(apiResponse.BodyText);

    //Tell FreeSwitch which events we are interested in
    await socket.SubscribeEvents(EventName.ChannelAnswer);

    //Handle events as they come in using Rx
    socket.ChannelEvents.Where(x => x.EventName == EventName.ChannelAnswer)
          .Subscribe(async x =>
          {
              Console.WriteLine("Channel Answer Event " + x.UUID);

              //we have a channel id, now we can control it
              await socket.Play(x.UUID, "/tmp/audioFinal1.wav");
          });

    Console.WriteLine("Press [Enter] to exit.");
    await Util.WaitForEnterKeyPress();
}