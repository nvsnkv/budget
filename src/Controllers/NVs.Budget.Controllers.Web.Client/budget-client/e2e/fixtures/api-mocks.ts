import { Page, Route } from '@playwright/test';

type MockOwner = {
  id: string;
  name: string;
};

type MockBudget = {
  id: string;
  name: string;
  version: string;
  owners: MockOwner[];
  taggingCriteria: unknown[];
  transferCriteria: unknown[];
  logbookCriteria: unknown[];
};

type AuthMockOptions = {
  budgets?: MockBudget[];
  owner?: MockOwner;
};

const defaultOwner: MockOwner = {
  id: 'owner-1',
  name: 'E2E User'
};

export const defaultBudgets: MockBudget[] = [
  {
    id: 'budget-1',
    name: 'Household',
    version: 'v1',
    owners: [defaultOwner],
    taggingCriteria: [],
    transferCriteria: [],
    logbookCriteria: []
  }
];

async function json(route: Route, data: unknown, status = 200): Promise<void> {
  await route.fulfill({
    status,
    contentType: 'application/json',
    body: JSON.stringify(data)
  });
}

export async function mockAuthenticatedApi(page: Page, options: AuthMockOptions = {}): Promise<void> {
  const owner = options.owner ?? defaultOwner;
  const budgets = [...(options.budgets ?? defaultBudgets)];
  let createdBudgetCounter = 0;

  await page.route('**/api/config', route => json(route, { apiUrl: 'https://localhost:7237' }));

  await page.route('**/auth/whoami', route =>
    json(route, {
      isAuthenticated: true,
      user: { id: owner.id },
      owner
    })
  );

  await page.route('**/api/v0.1/budget', async route => {
    const req = route.request();
    if (req.method() === 'GET') {
      await json(route, budgets);
      return;
    }

    if (req.method() === 'POST') {
      createdBudgetCounter += 1;
      const payload = req.postDataJSON() as { name?: string };
      const createdBudget: MockBudget = {
        id: `budget-created-${createdBudgetCounter}`,
        name: payload.name?.trim() || 'Untitled',
        version: 'v1',
        owners: [owner],
        taggingCriteria: [],
        transferCriteria: [],
        logbookCriteria: []
      };

      budgets.push(createdBudget);
      await json(route, createdBudget, 201);
      return;
    }

    await route.fallback();
  });

  await page.route('**/api/v0.1/budget/*/operations*', route => json(route, []));
}
