/**
 * Date and time formatting utilities
 */

/**
 * Formats a date to a short format (e.g., "Jan 15, 2024")
 */
export function formatDate(date: Date | string): string {
  return new Date(date).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  });
}

/**
 * Formats a date to a long format (e.g., "January 15, 2024")
 */
export function formatDateLong(date: Date | string): string {
  return new Intl.DateTimeFormat('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }).format(new Date(date));
}

/**
 * Formats a date to a full format with month name (e.g., "January 15, 2024")
 */
export function formatDateFull(date: Date | string): string {
  return new Date(date).toLocaleDateString('en-US', {
    month: 'long',
    day: 'numeric',
    year: 'numeric'
  });
}

/**
 * Formats a time to a readable format (e.g., "2:30 PM")
 */
export function formatTime(timestamp: Date | string): string {
  return new Date(timestamp).toLocaleTimeString('en-US', {
    hour: 'numeric',
    minute: '2-digit',
    hour12: true
  });
}

/**
 * Formats a date and time together (e.g., "Jan 15, 2024 at 2:30 PM")
 */
export function formatDateTime(date: Date | string): string {
  return `${formatDate(date)} at ${formatTime(date)}`;
}

/**
 * Formats a relative time (e.g., "2 days ago", "3 weeks ago")
 */
export function formatLastVisit(createdAt: Date | string): string {
  const now = new Date();
  const diffTime = Math.abs(now.getTime() - new Date(createdAt).getTime());
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  
  if (diffDays === 0) return "Today";
  if (diffDays === 1) return "1 day ago";
  if (diffDays < 7) return `${diffDays} days ago`;
  if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`;
  if (diffDays < 365) return `${Math.floor(diffDays / 30)} months ago`;
  return `${Math.floor(diffDays / 365)} years ago`;
}

/**
 * Formats a date to ISO string for API calls
 */
export function formatDateISO(date: Date | string): string {
  return new Date(date).toISOString();
}

