import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

// Re-export all utility functions for convenience
export * from "./utils/date"
export * from "./utils/prescription"
export * from "./utils/patient"
export * from "./utils/format"