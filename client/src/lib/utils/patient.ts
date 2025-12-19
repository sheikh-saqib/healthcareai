/**
 * Patient-related utilities
 */

/**
 * Gets patient initials from a full name
 * @param name - Full name of the patient
 * @returns First letter of each word, up to 2 letters, uppercase
 */
export function getPatientInitials(name: string): string {
  if (!name || name.trim().length === 0) {
    return '??';
  }
  
  return name
    .split(" ")
    .filter(n => n.length > 0)
    .map(n => n[0])
    .join("")
    .toUpperCase()
    .substring(0, 2);
}

/**
 * Formats patient name for display
 */
export function formatPatientName(name: string): string {
  if (!name) return 'Unknown Patient';
  return name.trim();
}

