import { broadcastChannel, clientManager, interopQueen } from "./globals";

import { v4 as uuidv4 } from "uuid";

export const measureText = await document.fonts.ready.then(() => {
  const canvas = document.createElement("canvas");
  const context = canvas.getContext("2d");
  return (text, cssFontString) => {
    context.font = cssFontString;
    console.log(context.font, document.fonts.check(context.font));
    return context.measureText(text).width;
  };
});

export const runAsync = (func) => new Promise((resolve) => resolve(func()));
export const sleep = (seconds) =>
  new Promise((resolve) => setTimeout(resolve, seconds));

export const broadcastMessageLog = (handler) => {
  return (event) => {
    console.debug(`> broadcast.${event.target.name.substring(0, 5)}`, event);
    handler(event);
  };
};

export const broadcast = (message, channel = broadcastChannel) => {
  message.from = clientManager.id;
  console.debug(`< broadcast.${channel.name.substring(0, 5)}`, message);
  channel.postMessage(message);
};

export const postMessageTo = (message, to, requestResponse = false) =>
  new Promise((resolve) => {
    const receiverChannel = new BroadcastChannel(to);

    if (!requestResponse) {
      broadcast(message, receiverChannel);
      return resolve();
    }

    message.replyTo = uuidv4();
    const channel = new BroadcastChannel(message.replyTo);
    const handler = broadcastMessageLog((event) => {
      channel.removeEventListener("message", handler);
      resolve(event.data);
    });
    channel.addEventListener("message", handler);
    broadcast(message, receiverChannel);
  });

export const handleMinimize = () => {
  interopQueen.Minimize();
};

export const handleMaximize = () => {
  interopQueen.Maximize();
};

export const handleRestore = () => {
  interopQueen.Restore();
};

export const handleClose = () => {
  interopQueen.Close();
};
