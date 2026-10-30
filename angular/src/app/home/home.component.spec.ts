import { AuthService, QUEUE_MANAGER } from '@abp/ng.core';
import { CoreTestingModule } from '@abp/ng.core/testing';
import { ThemeSharedTestingModule } from '@abp/ng.theme.shared/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NgxValidateCoreModule } from '@ngx-validate/core';
import { of, throwError } from 'rxjs';
import { DashboardService } from '../proxy/dashboard/dashboard.service';
import { DashboardDto } from '../proxy/dashboard/models';
import { HomeComponent } from './home.component';

describe('HomeComponent', () => {
  let fixture: ComponentFixture<HomeComponent>;
  let authService: { isAuthenticated: boolean; navigateToLogin: jasmine.Spy };
  let dashboardService: jasmine.SpyObj<DashboardService>;

  const dashboard: DashboardDto = {
    todaySalesTotal: 250,
    todayOrderCount: 2,
    todayRefundTotal: 0,
    openPurchaseOrderCount: 1,
    totalProducts: 3,
    lowStockCount: 1,
    inventoryValue: 1000,
    salesLast7Days: [
      { date: '2026-10-29', total: 50, orderCount: 1 },
      { date: '2026-10-30', total: 200, orderCount: 1 },
    ],
    topProducts: [
      { productName: 'Cola', quantitySold: 4, revenue: 40 },
      { productName: 'Chips', quantitySold: 8, revenue: 80 },
    ],
    lowStockProducts: [{ sku: 'COLA-330', name: 'Cola', quantityOnHand: 1, reorderThreshold: 5 }],
  };

  function create(isAuthenticated: boolean) {
    authService.isAuthenticated = isAuthenticated;
    fixture = TestBed.createComponent(HomeComponent);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  beforeEach(async () => {
    authService = { isAuthenticated: false, navigateToLogin: jasmine.createSpy('navigateToLogin') };
    dashboardService = jasmine.createSpyObj('DashboardService', ['get']);
    dashboardService.get.and.returnValue(of(dashboard));

    await TestBed.configureTestingModule({
      declarations: [HomeComponent],
      imports: [CoreTestingModule.withConfig(), ThemeSharedTestingModule.withConfig(), NgxValidateCoreModule],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: DashboardService, useValue: dashboardService },
        // abpPermission in the template needs a queue manager the testing module does not provide.
        { provide: QUEUE_MANAGER, useValue: { init: () => undefined, addTask: () => undefined } },
      ],
    }).compileComponents();
  });

  it('does not load the dashboard when logged out', () => {
    const component = create(false);

    expect(component.hasLoggedIn).toBeFalse();
    expect(dashboardService.get).not.toHaveBeenCalled();
  });

  it('sends the user to the login page', () => {
    create(false).login();

    expect(authService.navigateToLogin).toHaveBeenCalled();
  });

  it('loads the dashboard when logged in', () => {
    const component = create(true);

    expect(dashboardService.get).toHaveBeenCalledTimes(1);
    expect(component.dashboard).toBe(dashboard);
    expect(component.isLoading).toBeFalse();
  });

  it('stops loading when the dashboard request fails', () => {
    dashboardService.get.and.returnValue(throwError(() => new Error('boom')));

    const component = create(true);

    expect(component.dashboard).toBeNull();
    expect(component.isLoading).toBeFalse();
  });

  it('scales chart bars relative to the largest value', () => {
    const component = create(true);

    expect(component.barHeight(200)).toBe(100);
    expect(component.barHeight(50)).toBe(25);
    expect(component.topBarWidth(8)).toBe(100);
    expect(component.topBarWidth(4)).toBe(50);
  });

  it('avoids dividing by zero when there is no data', () => {
    const component = create(false);

    expect(component.maxDailySales).toBe(1);
    expect(component.maxTopQuantity).toBe(1);
    expect(component.barHeight(0)).toBe(0);
  });
});
