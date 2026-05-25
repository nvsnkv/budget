export function getBrowserTimeZone(): string {
  return Intl.DateTimeFormat().resolvedOptions().timeZone;
}

/**
 * UTC instant for the start of a calendar day (yyyy-MM-dd).
 * Uses Date.UTC so toISOString() yields T00:00:00.000Z on that date, not local midnight shifted by offset.
 */
export function calendarDateToUtcStart(dateStr: string): Date {
  const [year, month, day] = dateStr.split('-').map(Number);
  if (!Number.isFinite(year) || !Number.isFinite(month) || !Number.isFinite(day)) {
    return new Date(dateStr);
  }
  return new Date(Date.UTC(year, month - 1, day, 0, 0, 0, 0));
}

/** Exclusive upper bound for backend `Timestamp < till` filters (start of next UTC calendar day). */
export function calendarDateToUtcExclusiveEnd(dateStr: string): Date {
  const [year, month, day] = dateStr.split('-').map(Number);
  if (!Number.isFinite(year) || !Number.isFinite(month) || !Number.isFinite(day)) {
    return new Date(dateStr);
  }
  return new Date(Date.UTC(year, month - 1, day + 1, 0, 0, 0, 0));
}
