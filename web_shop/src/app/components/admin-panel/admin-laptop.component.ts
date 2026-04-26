import { Component, inject, signal, OnInit } from '@angular/core';
import { LaptopService } from '../../services/laptop.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LaptopAdminDTO } from '../../DTO/laptop-dto';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin-laptop.component',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './admin-laptop.component.html',
  standalone: true,
})
export class AdminLaptopComponent implements OnInit {
  public _laptopService = inject(LaptopService);
  public _fb = inject(FormBuilder);

  public laptopForm = this._fb.nonNullable.group({
    id: [''],
    name: ['', Validators.required],
    price: [1, [Validators.required, Validators.min(1)]],
    img: ['', Validators.required],
    cpu: ['', Validators.required],
    ram: [1, [Validators.required, Validators.min(1)]],
    gpu: ['', Validators.required],
    diskSize: [''],
    screenSize: [1, [Validators.required, Validators.min(1)]],
    screenResolution: ['', Validators.required],
    screenRefresh: [1, [Validators.required, Validators.min(1)]],
    battery: [1, [Validators.required, Validators.min(1)]],
  });

  public isEdit = signal(false);

  ngOnInit() {
  }

  onSubmit() {
    if (this.laptopForm.invalid) {
      this.laptopForm.markAllAsTouched();
      return;
    }

    const formValue = this.laptopForm.getRawValue() as LaptopAdminDTO;

    this._laptopService.saveLaptop(formValue).subscribe({
      next: () => {
        this._laptopService.getAdminLaptops();
        this.resetForm();
      },
    });
  }

  editLaptop(laptop: LaptopAdminDTO) {
    this.isEdit.set(true);
    this.laptopForm.patchValue(laptop);
  }

  deleteLaptop(id: string) {
    if (!id) return;

    if (confirm('Are u sure?')) {
      this._laptopService.deleteLaptop(id).subscribe({
        next: () => {
          this._laptopService.getAdminLaptops();
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
      gpu: '',
      diskSize: '',
      screenSize: 1,
      screenResolution: '',
      screenRefresh: 1,
      battery: 1
    });
  }
}
