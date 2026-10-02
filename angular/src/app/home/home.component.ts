import { AuthService } from '@abp/ng.core';
import { Component, OnInit, inject } from '@angular/core';
import { DashboardService } from '../proxy/dashboard/dashboard.service';
import { DashboardDto } from '../proxy/dashboard/models';

@Component({
  standalone: false,
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  private authService = inject(AuthService);
  private dashboardService = inject(DashboardService);

  dashboard: DashboardDto | null = null;
  isLoading = false;

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated;
  }

  get maxDailySales(): number {
    const totals = this.dashboard?.salesLast7Days.map(d => d.total) ?? [];
    return Math.max(...totals, 1);
  }

  get maxTopQuantity(): number {
    const quantities = this.dashboard?.topProducts.map(t => t.quantitySold) ?? [];
    return Math.max(...quantities, 1);
  }

  ngOnInit() {
    if (this.hasLoggedIn) {
      this.loadDashboard();
    }
  }

  loadDashboard() {
    this.isLoading = true;
    this.dashboardService.get().subscribe({
      next: result => {
        this.dashboard = result;
        this.isLoading = false;
      },
      error: () => (this.isLoading = false),
    });
  }

  barHeight(total: number): number {
    return Math.round((total / this.maxDailySales) * 100);
  }

  topBarWidth(quantity: number): number {
    return Math.round((quantity / this.maxTopQuantity) * 100);
  }

  login() {
    this.authService.navigateToLogin();
  }
}
