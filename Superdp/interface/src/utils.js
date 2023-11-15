import { broadcastChannel } from "./globals";

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

export const broadcast = (message, requestResponse = false) =>
  new Promise((resolve) => {
    if (!requestResponse) {
      broadcastChannel.postMessage(message);
      return resolve();
    }

    message.replyTo = uuidv4();
    const channel = new BroadcastChannel(message.replyTo);
    const handler = ({ data }) => {
      channel.removeEventListener("message", handler);
      resolve(data);
    };
    channel.addEventListener("message", handler);
    broadcastChannel.postMessage(message);
  });
