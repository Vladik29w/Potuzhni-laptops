import { Component, inject, signal, OnInit } from '@angular/core';
import { AdminService } from '../../services/admin.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LaptopAdminDTO } from '../../DTO/laptop-dto';
import { OrderStatsDTO } from '../../DTO/order-dto';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-admin-panel.component',
  imports: [ReactiveFormsModule],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css',
  standalone: true,
})
export class AdminPanelComponent implements OnInit {
  public _adminService = inject(AdminService);
  public _fb = inject(FormBuilder);

  public statChart: any;
  public statDays = signal(7);

  public laptopForm = this._fb.nonNullable.group({
    id: [''],
    name: ['', Validators.required],
    price: [1, [Validators.required, Validators.min(1)]],
    img: ['', Validators.required],
    cpu: ['', Validators.required],
    ram: [1, [Validators.required, Validators.min(1)]],
    gpu: ['', Validators.required]
  });

  public isEdit = signal(false);

  ngOnInit() {
    this.loadStats();
  }

  onSubmit() {
    if (this.laptopForm.invalid) {
      this.laptopForm.markAllAsTouched();
      return;
    }

    const formValue = this.laptopForm.getRawValue() as LaptopAdminDTO;

    this._adminService.saveLaptop(formValue).subscribe({
      next: () => {
        this._adminService.loadLaptops();
        this.resetForm();
      },
      error: (err) => {
        console.error('Помилка при збереженні:', err);
      }
    });
  }

  editLaptop(laptop: LaptopAdminDTO) {
    this.isEdit.set(true);
    this.laptopForm.patchValue(laptop);
  }

  deleteLaptop(id: string) {
    if (!id) return;

    if (confirm('Are u sure?')) {
      this._adminService.deleteLaptop(id).subscribe({
        next: () => {
          this._adminService.loadLaptops();
        }
      });
    }
  }

  resetForm() {
    this.isEdit.set(false);
    this.laptopForm.reset({
      id: '',
      name: '',
      price: 1,
      img: '',
      cpu: '',
      ram: 1,
      gpu: ''
    });
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
    });
  }
}
