import { expect, test } from '@playwright/test';
import { mockAuthenticatedApi } from '../fixtures/api-mocks';

test.describe('taiga form smoke', () => {
  test('creates a budget and navigates to operations', async ({ page }) => {
    await mockAuthenticatedApi(page);
    await page.goto('/budget/new', { waitUntil: 'domcontentloaded' });

    await page.getByTestId('new-budget-name-input').fill('Vacation');
    await page.getByTestId('new-budget-submit').click();

    await expect(page).toHaveURL(/\/budget\/budget-created-1\/operations$/);
    await expect(page.getByTestId('operations-page-title')).toHaveText('Operations');
    await expect(page.getByTestId('budget-selector-trigger')).toContainText('Vacation');
  });
});
