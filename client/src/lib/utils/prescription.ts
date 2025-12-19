/**
 * Prescription-related utilities
 */

/**
 * Gets the badge variant for a prescription status
 */
export function getPrescriptionBadgeVariant(status: string): 'default' | 'destructive' | 'secondary' | 'outline' {
  switch (status) {
    case 'approved':
      return 'default';
    case 'rejected':
      return 'destructive';
    case 'pending':
      return 'secondary';
    default:
      return 'outline';
  }
}

/**
 * Gets the badge color classes for a prescription status
 */
export function getPrescriptionBadgeColor(status: string): string {
  switch (status) {
    case 'approved':
      return 'bg-green-100 text-green-800';
    case 'rejected':
      return 'bg-red-100 text-red-800';
    case 'pending':
      return 'bg-orange-100 text-orange-800';
    default:
      return 'bg-gray-100 text-gray-800';
  }
}

/**
 * Formats prescription status to display text (capitalizes first letter)
 */
export function formatPrescriptionStatus(status: string): string {
  return status.charAt(0).toUpperCase() + status.slice(1);
}

/**
 * Formats medications string into a readable format
 * Handles JSON parsing and formatting
 */
export function formatMedications(medications: string): string {
  try {
    const parsed = typeof medications === 'string' ? JSON.parse(medications) : medications;
    
    if (Array.isArray(parsed)) {
      return parsed.map((med: any) => {
        if (typeof med === 'string') return med;
        if (med.name) {
          let result = med.name;
          if (med.dosage) result += ` - ${med.dosage}`;
          if (med.frequency) result += ` (${med.frequency})`;
          return result;
        }
        return JSON.stringify(med);
      }).join(', ');
    }
    
    if (typeof parsed === 'object' && parsed !== null) {
      return Object.entries(parsed)
        .map(([key, value]) => `${key}: ${value}`)
        .join(', ');
    }
    
    return medications;
  } catch {
    return medications;
  }
}

