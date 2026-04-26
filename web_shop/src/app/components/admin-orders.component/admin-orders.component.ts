import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { OrderDTO, OrderStatsDTO } from '../../DTO/order-dto';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-admin-orders.component',
  imports: [CommonModule],
  templateUrl: './admin-orders.component.html',
  standalone: true,
})
export class AdminOrdersComponent implements OnInit {
  public _orderService = inject(OrderService);
  public statChart: any;
  public statDays = signal(7);
  public orders = signal<OrderDTO[]>([]);
  public page = signal(1);
  public pageSize = 12;
  public totalCount = signal(0);
  public totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  ngOnInit() {
    this.loadStats();
    this.loadOrders();
  }



  loadStats() {
    this._orderService.getOrderStats(this.statDays()).subscribe({
      next: (data) => {
        this.renderChart(data);
      }
    });
  }

  loadOrders() {
    this._orderService.getAllOrders(this.page(), this.pageSize).subscribe({
      next: (data) => {
        this.orders.set(data.items);
        this.totalCount.set(data.totalCount);
      }
    });
  }

  previousPage() {
    if (this.page() <= 1) {
      return;
    }

    this.page.update(value => value - 1);
    this.loadOrders();
  }

  nextPage() {
    if (this.page() >= this.totalPages()) {
      return;
    }

    this.page.update(value => value + 1);
    this.loadOrders();
  }

  confirmOrder(orderId: string) {
    this._orderService.confirmOrder(orderId).subscribe({
      next: () => {
        this.orders.update(orders =>
          orders.map(order =>
            order.id === orderId ? { ...order, isConfirmed: true } : order
          )
        );
      }
    });
  }

  changeStatsPeriod(days: number) {
    this.statDays.set(days);
    this.loadStats();
  }

  renderChart(data: OrderStatsDTO[]) {
    if (this.statChart) {
      this.statChart.destroy();
    }

    const labels = data.map(item => new Date(item.date).toLocaleDateString());
    const sums = data.map(item => item.sum);
    const quantities = data.map(item => item.quantity);

    this.statChart = new Chart('salesChart', {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Sales total sum',
            data: sums,
            backgroundColor: 'rgba(54, 162, 235, 0.5)',
            borderColor: 'rgba(54, 162, 235, 1)',
            borderWidth: 1,
            yAxisID: 'y'
          },
          {
            label: 'Sales quantity',
            data: quantities,
            type: 'line',
            backgroundColor: 'rgba(255, 99, 132, 0.5)',
            borderColor: 'rgba(255, 99, 132, 1)',
            borderWidth: 2,
            yAxisID: 'y1'
          }
        ]
      },
      options: {
        responsive: true,
        scales: {
          y: { type: 'linear', display: true, position: 'left' },
          y1: { type: 'linear', display: true, position: 'right', grid: { drawOnChartArea: false } }
        }
      }
    });
  }
}
