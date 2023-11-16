import { clientManager, interopQueen } from "./globals";

export const provideData = async () => {
  const creationTimestamp = await interopQueen.Init(clientManager.id);
  const elapsedTime = Date.now() - creationTimestamp;
  console.log(`Startup took ${elapsedTime}ms`);
};
