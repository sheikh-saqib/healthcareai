import { createRoot } from "react-dom/client";
import "./lib/syncfusion-license"; // Initialize Syncfusion license
import App from "./App";
import "./index.css";

createRoot(document.getElementById("root")!).render(<App />);
