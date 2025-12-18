// Syncfusion License Initialization
import { registerLicense } from '@syncfusion/ej2-base';

export function initializeSyncfusionLicense() {
  // Register Syncfusion license if available
  const licenseKey = import.meta.env.VITE_SYNCFUSION_LICENSE_KEY;
  if (licenseKey) {
    registerLicense(licenseKey);
    console.log('Syncfusion license registered successfully');
  } else {
    console.warn('Syncfusion license key not found. PDF generation may have limitations.');
  }
}

// Initialize on module load
initializeSyncfusionLicense();