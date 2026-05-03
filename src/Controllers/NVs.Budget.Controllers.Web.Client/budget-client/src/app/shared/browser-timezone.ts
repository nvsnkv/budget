/** IANA id from the browser (e.g. Europe/Moscow). Safe fallback if APIs are unavailable. */
export function browserIanaTimeZoneId(): string {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone ?? 'UTC';
  } catch {
    return 'UTC';
  }
}
