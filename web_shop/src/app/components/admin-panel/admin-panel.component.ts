import { Component, inject, signal } from '@angular/core';
import { AdminService } from '../../services/admin.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LaptopAdminDTO } from '../../DTO/laptop-dto';
import { OrderStatsDTO } from '../../DTO/order-dto';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables)

@Component({
  selector: 'app-admin-panel.component',
  imports: [ReactiveFormsModule],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css',
  standalone: true,
})
export class AdminPanelComponent {
  public _adminService = inject(AdminService);
  public _fb = inject(FormBuilder);

  public statChart: any;
  public statDays = signal(7);

  public laptopForm = this._fb.group({
    id: ['', Validators.required],
    name: ['', Validators.required],
    price: [1, Validators.required],
    img: ['', Validators.required],
    cpu: ['', Validators.required],
    ram: [1, Validators.required],
    gpu: ['', Validators.required]
  })

  public isEdit = signal(false);

  ngOnInit() {
    this.loadStats();
  }

  onSubmit() {
    if (this.laptopForm.invalid) return;

    const formValue = this.laptopForm.value as LaptopAdminDTO;

    this._adminService.saveLaptop(formValue).subscribe({
      next: () => {
        this._adminService.loadLaptops();
        this.resetForm();
      }
    })
  }
  editLaptop(laptop: LaptopAdminDTO) {
    this.isEdit.set(true);
    this.laptopForm.patchValue(laptop);
  }

  deleteLaptop(id: string) {
    if (!id) return

    if (confirm('Are u sure ?')) {
      this._adminService.deleteLaptop(id).subscribe({
        next: () => {
          this._adminService.loadLaptops();
        }
      })
    }
  }

  resetForm() {
    this.isEdit.set(false);
    this.laptopForm.reset();
  }

  loadStats() {
    this._adminService.getOrderStats(this.statDays()).subscribe({
      next: (data) => {
        this.renderChart(data);
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
    })
  }
}
