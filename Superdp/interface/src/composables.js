import { clientManager, interopQueen, windowIsMaximized } from "./globals";

export const provideData = async () => {
  const { creationTimestamp, formBorderStyle } = JSON.parse(
    await interopQueen.Init(clientManager.id)
  );
  const elapsedTime = Date.now() - creationTimestamp;
  console.log(`Startup took ${elapsedTime}ms`, formBorderStyle);
  windowIsMaximized.value = formBorderStyle == 0;
};
