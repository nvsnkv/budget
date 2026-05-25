import { calendarDateToUtcExclusiveEnd, calendarDateToUtcStart } from './date-api.utils';

describe('date-api.utils calendar → UTC', () => {
  it('calendarDateToUtcStart serializes as UTC midnight on the same calendar day', () => {
    expect(calendarDateToUtcStart('2026-06-15').toISOString()).toBe('2026-06-15T00:00:00.000Z');
  });

  it('calendarDateToUtcExclusiveEnd serializes as UTC midnight on the next calendar day', () => {
    expect(calendarDateToUtcExclusiveEnd('2026-06-15').toISOString()).toBe('2026-06-16T00:00:00.000Z');
  });

  it('handles month boundaries in UTC', () => {
    expect(calendarDateToUtcStart('2026-12-31').toISOString()).toBe('2026-12-31T00:00:00.000Z');
    expect(calendarDateToUtcExclusiveEnd('2026-12-31').toISOString()).toBe('2027-01-01T00:00:00.000Z');
  });
});
