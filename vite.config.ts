import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";
import runtimeErrorOverlay from "@replit/vite-plugin-runtime-error-modal";

// Get the current directory
const currentDir = path.resolve();

export default defineConfig({
  plugins: [
    react(),
    runtimeErrorOverlay(),
  ],
  resolve: {
    alias: {
      "@": path.resolve(currentDir, "client", "src"),
      "@shared": path.resolve(currentDir, "shared"),
      "@assets": path.resolve(currentDir, "attached_assets"),
    },
  },
  root: path.resolve(currentDir, "client"),
  build: {
    outDir: path.resolve(currentDir, "dist/public"),
    emptyOutDir: true,
  },
  server: {
    port: 3000,
    proxy: {
      "/api": {
        target: "http://localhost:5044",
        changeOrigin: true,
        secure: false,
      },
      "/health": {
        target: "http://localhost:5044",
        changeOrigin: true,
        secure: false,
      },
      "/activityHub": {
        target: "http://localhost:5044",
        changeOrigin: true,
        secure: false,
        ws: true, // Enable WebSocket proxying
      },
      "/swagger": {
        target: "http://localhost:5044",
        changeOrigin: true,
        secure: false,
      }
    },
    fs: {
      strict: true,
      deny: ["**/.*"],
    },
  },
});
