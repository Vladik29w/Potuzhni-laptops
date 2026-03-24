import { Component, inject, signal } from '@angular/core';
import { AdminService } from '../../services/admin.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LaptopAdminDTO } from '../../DTO/laptop-dto';

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
}
