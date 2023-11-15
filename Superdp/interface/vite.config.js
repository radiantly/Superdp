import { fileURLToPath, URL } from "node:url";

import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    // support for https://caniuse.com/css-has
    target: "edge105",

    // separate source maps aren't supported by webview2 yet.
    // see https://github.com/MicrosoftEdge/WebView2Feedback/issues/961
    sourcemap: "inline",
  },
  plugins: [vue()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
});
