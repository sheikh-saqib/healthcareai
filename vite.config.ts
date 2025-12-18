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
      // Explicitly resolve Font Awesome from project root
      "@fortawesome/react-fontawesome": path.resolve(currentDir, "node_modules", "@fortawesome", "react-fontawesome"),
      "@fortawesome/fontawesome-svg-core": path.resolve(currentDir, "node_modules", "@fortawesome", "fontawesome-svg-core"),
      "@fortawesome/free-solid-svg-icons": path.resolve(currentDir, "node_modules", "@fortawesome", "free-solid-svg-icons"),
    },
    // Include project root node_modules in resolution
    dedupe: ["bootstrap", "@fortawesome/react-fontawesome", "@fortawesome/fontawesome-svg-core"],
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
      strict: false,
      allow: [".."],
    },
  },
});
