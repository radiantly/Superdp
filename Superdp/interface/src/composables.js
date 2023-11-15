import { onBeforeUnmount, onMounted } from "vue";

import { clientManager, interopQueen } from "./globals";
import { TabManager } from "./classes/TabManager";

export const provideData = async () => {
  const creationTimestamp = await interopQueen.Init();
  const elapsedTime = Date.now() - creationTimestamp;
  console.log(`Startup took ${elapsedTime}ms`);
};
